using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;

namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// Type de transaction
    /// </summary>
    public enum TransactionTypeEnum
    {   /// <summary>
        /// La transaction est ouverte, validée (oui/non) en interne.
        /// </summary>
        Internal,
        /// <summary>
        /// La transaction est ouverte, validée (oui/non) par des delegués.
        /// </summary>
        External
    }
    
    /// <summary>
    ///  Delegate pour définir les exceptions acceptées pour retenter une méthode
    /// </summary>
    /// <typeparam name="Exception"></typeparam>
    /// <typeparam name="String"></typeparam>
    /// <param name="ex">Représente l'exception</param>
    /// <param name="mesage">retourne un message court qui représente l'exception (pour le log)</param>
    /// <returns></returns>
    /// FI 20211206 [XXXXX] Add
    public delegate Boolean IsRetryException<Exception, String>(Exception ex, out string mesage);


    /// <summary>
    /// Classe permettant de (re)tenter n fois une méthode si
    /// <para>- deadlock(s) ou des timeout(s) ou </para>
    /// <para>- exception(s) particulière(s) (InitIsRetryException permet de définir les exceptions)</para>
    /// <para>Par défaut 10 tentatives dans un délai imparti de 120 secondes</para>
    /// </summary>
    /// FI 20210121 [XXXXX] Add
    public partial class  TryMultiple
    {
        /// <summary>
        ///  Delegué pour ouverture de la transaction
        /// </summary>
        public Action BeginTranAction;
        /// <summary>
        ///  Delegué pour validation de la transaction
        /// </summary>
        public Action CommitTranAction;
        /// <summary>
        ///  Delegué pour invalidée de la transaction
        /// </summary>
        public Action RollbackTranAction;

        /// <summary>
        /// 
        /// </summary>
        public TransactionTypeEnum TransactionType
        {
            get;
            set;
        }


        /// <summary>
        /// Accès à la base de données
        /// </summary>
        public string CS
        {
            get;
            private set;
        }

        /// <summary>
        ///  Préfixe pour accéder aux clés éventuellement présentes dans le fichier de configuration
        ///  <para>Permet d'overrider le nombre maximum de tentatives ou le timeout</para>
        /// </summary>
        public string AppSettingKeyPrefix
        {
            get;
            private set;
        }

        /// <summary>
        /// Description de la méthode pour affichage dans le log
        /// </summary>
        public string ActionDesc
        {
            get;
            private set;
        }

        /// <summary>
        /// si Renseigné, au delà de 5 tentatives (6 et plus), le traitement termine en warning
        /// </summary>
        public SetErrorWarning SetErrorWarning
        {
            get;
            set;
        }

        /// <summary>
        /// Si Renseigné Alimentation du log via ancienne méthode (Ne peut être utilisé que côté web)
        /// </summary>
        public LogAddDetail LogAddDetail
        {
            set;
            get;
        }


        /// <summary>
        /// Delai max pour exécuter la méthode n fois lorsque des DeadLock et Timeout sont rencontrés. Le temps doit être exprimé en secondes. 
        /// <para>Cette propriété peut être écrasée via config (clé : {AppSettingKey}_timeoutAttempt)</para>
        /// <para>Si non renseignée, la valeur accordée est 120 secondes</para>
        /// </summary>
        public Nullable<int> Timeout
        {
            get;
            set;
        }


        /// <summary>
        /// Nombre de tentatives max pour exécuter la méthode lorsque des DeadLock et Timeout sont rencontrés 
        /// <para>Cette propriété peut être écrasée via config (clé : {AppSettingKey}_maxAttempt)</para>
        ///<para>Si non renseignée, la valeur retenue est 10 tentatives</para>
        /// </summary>
        public Nullable<int> MaxAttemptNumber
        {
            get;
            set;
        }

        /// <summary>
        ///  Si Renseigné, bloque le thread principale entre 2 tentatives (uniqument si DeadLock rencontré). Le temps doit être exprimé en secondes.
        /// <para>Si non renseigné, il n'y a pas de bloquage</para>
        /// </summary>
        public Nullable<int> ThreadSleep
        {
            get;
            set;
        }

        /// <summary>
        /// Si True, ouverture une transaction avant l'appel à la méthode. 
        /// <para>si TransactionType est Internal, la transaction est ouverte et validée (oui/non) en interne. La méthode doit obligatoirement posséder un paramètre IDbTransaction non alimenté (null). La transaction ouverte est alors utilisée lors de l'appel de la méthode</para>
        /// <para>si TransactionType est External, la transaction est ouverte et validée (oui/non) via des delegues. La méthode ne doit pas posseder un paramètre IDbTransaction</para>
        /// </summary>
        public Boolean IsModeTransactional
        {
            get;
            set;
        }

        /// <summary>
        ///  Méthode utilisée pour vérifier qu'une exception est acceptée pour une nouvelle tentative   
        /// </summary>
        /// FI 20211206 [XXXXX] Add
        private IsRetryException<Exception, String> _isRetry;


        /// <summary>
        ///  Obtient ou définie le niveau hiérarchique appliqué aux entrées du log
        /// </summary>
        /// FI 20211206 [XXXXX] Add
        public int LogRankOrder
        {
            get;
            set;
        }

        /// <summary>
        ///  Obtient ou définie un header qui sera appliqué à chaque entrée du log
        /// </summary>
        /// FI 20211206 [XXXXX] Add
        public string LogHeader
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="appSettingKeyPrefix"></param>
        /// <param name="processDesc"></param>
        public TryMultiple(string cs, string appSettingKeyPrefix, string processDesc)
        {
            CS = cs;
            AppSettingKeyPrefix = appSettingKeyPrefix;
            ActionDesc = processDesc;
            IsModeTransactional = false;
            LogRankOrder = 3;
            LogHeader = string.Empty;
        }

        /// <summary>
        /// Excécution d'une méthode dépourvue de paramètre et retournant aucune valeur
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public void Exec(Action action)
        {
            Exec<Action>(action);
        }

        /// <summary>
        /// Excécution d'une méthode avec 1 paramètre et retournant aucune valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public void Exec<T>(Action<T> action, T arg)
        {
            Exec<Action<T>>(action, new Object[] { arg });
        }

        /// <summary>
        /// Excécution d'une méthode avec 2 paramètres et retournant aucune valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public void Exec<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            Exec<Action<T1, T2>>(action, new Object[] { arg1, arg2 });
        }

        /// <summary>
        /// Excécution d'une méthode avec 3 paramètres et retournant aucune valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public void Exec<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T2 arg3)
        {
            Exec<Action<T1, T2, T3>>(action, new Object[] { arg1, arg2, arg3 });
        }

        /// <summary>
        /// Excécution d'une méthode avec 4 paramètres et retournant aucune valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public void Exec<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T2 arg3, T4 arg4)
        {
            Exec<Action<T1, T2, T3, T4>>(action, new Object[] { arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// Excécution d'une méthode avec 5 paramètres ne retournant aucune valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        /// EG 20210916 [XXXXX] New
        public void Exec<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T2 arg3, T4 arg4, T5 arg5)
        {
            Exec<Action<T1, T2, T3, T4, T5>>(action, new Object[] { arg1, arg2, arg3, arg4, arg5 });
        }
        /// <summary>
        /// Excécution d'une méthode avec 0 paramètre et retournant une valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public TResult Exec<TResult>(Func<TResult> action)
        {
            return (TResult)Exec<Func<TResult>>(action);
        }

        /// <summary>
        /// Excécution d'une méthode avec 1 paramètre et retournant une valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public TResult Exec<T1, TResult>(Func<T1, TResult> action, T1 arg)
        {
            return (TResult)Exec<Func<T1, TResult>>(action, new Object[] { arg });
        }

        /// <summary>
        /// Excécution d'une méthode avec 2 paramètres et retournant une valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        public TResult Exec<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 arg, T2 arg2)
        {
            return (TResult)Exec<Func<T1, T2, TResult>>(action, new Object[] { arg, arg2 });
        }

        /// <summary>
        /// Excécution d'une méthode avec 3 paramètres et retournant une valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        /// FI 20210222 [XXXXX] Add
        public TResult Exec<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> action, T1 arg, T2 arg2, T3 arg3)
        {
            return (TResult)Exec<Func<T1, T2, T3, TResult>>(action, new Object[] { arg, arg2, arg3 });
        }

        /// <summary>
        /// Excécution d'une méthode avec 5 paramètres et retournant une valeur
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        /// EG 20210916 [XXXXX] New
        public TResult Exec<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> action, T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return (TResult)Exec<Func<T1, T2, T3, T4, T5, TResult>>(action, new Object[] { arg, arg2, arg3, arg4, arg5 });
        }

        private IDbTransaction dbTransaction = null;
        /// <summary>
        /// Excécution de la méthode (de 1 à n tentatives dans un délai imparti) 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args">Paramètres éventuels de la méthode</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TryMultipleException"></exception>
        /// FI 20211206 [XXXXX] Refactoring => Possibilié de retenter une méthode en fonction d'une exception particulière
        private Object Exec<T>(T action, params object[] args) where T : Delegate
        {
            if (null == action)
                throw new ArgumentNullException("action", "action is null");

            Object ret = null;

            bool isToProcess = true;
            int nbProcess = 0;

            DatetimeProfiler dtProfiler = new DatetimeProfiler(DateTime.Now);
            //Delai max pour exécuter la méthode n fois lorsque des DeadLock et Timeout sont rencontrés 
            //Par ordre de priorité, Spheres considère la valeur présente dans le fichier de config, puis la propriété Timeout, puis la valeur par défaut 120)
            double timeOut = Convert.ToDouble(SystemSettings.GetAppSettings($"{AppSettingKeyPrefix}_timeoutAttempt", Timeout.HasValue ? Timeout.Value.ToString() : "120"));
            //nbr de tentative max pour exécuter la méthode lorsque des DeadLock et Timeout sont rencontrés 
            //Par ordre de priorité, Spheres considère la valeur présente dans le fichier de config, puis la propriété MaxAttemptNumber, puis la valeur par défaut 10)
            int nbMax = Convert.ToInt32(SystemSettings.GetAppSettings($"{AppSettingKeyPrefix}_maxAttempt", MaxAttemptNumber.HasValue ? MaxAttemptNumber.Value.ToString() : "10"));

            int warningLoop = 5; //nbr de tentatives qu delà duquel un warning est ajouté dans le Log

            while (isToProcess)
            {
                try
                {
                    nbProcess++;

                    if (IsModeTransactional)
                    {
                        CheckTransactionalMode(action);

                        BeginTran();

                        if (TransactionType == TransactionTypeEnum.Internal)
                            ApplyDbTransaction(action, dbTransaction, args);

                    }

                    ret = action.DynamicInvoke(args);

                    if (IsModeTransactional)
                    {
                        if ((null != ret) && ret.GetType().Equals(typeof(Cst.ErrLevel)))
                        {
                            Cst.ErrLevel errLevel = (Cst.ErrLevel)Enum.Parse(typeof(Cst.ErrLevel), ret.ToString());
                            if (errLevel != Cst.ErrLevel.FAILURE)
                                CommitTran();
                            else
                                RollbackTran();
                        }
                        else
                            CommitTran();
                    }

                    isToProcess = false;

                    if (nbProcess > 1) //si (nbProcess>1) => il y a erreur et retraitement
                    {
                        // message qui indique que la méthode s'est bien réroulée après retraitement
                        string msgLog = $"Action: <b>{ActionDesc}</b> successfully executed. Number of attempts: {nbProcess}.</b>";

                        // Au delà de {warningLoop} on n’est plus en INFO mais en WARNING  
                        if (nbProcess > warningLoop)
                        {
                            // si méthode en succès suite à plusieurs tentatives, le status final est éventuellement en warning (si le nbr de tentative > warningLoop)
                            SetErrorWarning?.Invoke(ProcessStateTools.StatusWarningEnum);
                            msgLog += Cst.CrLf + "<b>- contact your system administrator.</b>";
                        }

                        if (null != LogAddDetail)
                            LogAddDetail.Invoke((nbProcess > warningLoop) ? LogLevelEnum.Warning : LogLevelEnum.Info, SetMessageLog(msgLog), LogRankOrder);
                        Logger.Log(new LoggerData((nbProcess > warningLoop) ? LogLevelEnum.Warning : LogLevelEnum.Info, SetMessageLog(msgLog), LogRankOrder));
                    }

                }
                catch (Exception ex)
                {
                    double elapsedTime = dtProfiler.GetTimeSpan().TotalSeconds;

                    if (IsModeTransactional)
                        RollbackTran();

                    isToProcess = (elapsedTime.CompareTo(timeOut) == -1);
                    isToProcess &= (nbProcess < nbMax);

                    string message = string.Empty;
                    if (ex.GetType().Equals(typeof(TargetInvocationException)))
                    {
                        Boolean isRetry = IsRetryDefault(ex.InnerException, out message);
                        if ((false == isRetry) && (null != _isRetry))
                            isRetry = _isRetry.Invoke(ex.InnerException, out message);
                        isToProcess &= isRetry;
                    }
                    else
                    {
                        isToProcess = false;
                    }

                    if (isToProcess)
                    {
                        if (nbProcess == 1)
                        {
                            // Ecriture de l'erreur  (Niveau Info, visible si niveau de trace => 3 uniquement)
                            string msgErr = $"Action: <b>{ActionDesc}</b> in error." + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);

                            if (null != LogAddDetail)
                                LogAddDetail.Invoke(LogLevelEnum.Info, SetMessageLog(msgErr), LogRankOrder);
                            Logger.Log(new LoggerData(LogLevelEnum.Info, SetMessageLog(msgErr), LogRankOrder));

                            // Ecriture de l'info qui indique si retraitement  (Niveau Info, visible si niveau de trace => 3 uniquement)
                            string msgLog = $"Action: <b>{ActionDesc}</b> in error." + Cst.CrLf + StrFunc.AppendFormat("error occurred: <b>{0}</b>", message);
                            msgLog += Cst.CrLf + StrFunc.AppendFormat("<b>Spheres® {0} again.</b>", isToProcess ? "tries" : "doesn't try");

                            if (null != LogAddDetail)
                                LogAddDetail.Invoke(LogLevelEnum.Info, SetMessageLog(msgLog), LogRankOrder);
                            Logger.Log(new LoggerData(LogLevelEnum.Info, SetMessageLog(msgLog), LogRankOrder));
                        }

                        if (ThreadSleep.HasValue)
                            Thread.Sleep(ThreadSleep.Value * 1000);

                    }
                    else
                    {
                        string msgAbort = $"Action: <b>{ActionDesc}</b> aborted.";
                        if (nbProcess > 1)
                            msgAbort += Cst.CrLf + StrFunc.AppendFormat("Number of attempts: {0}.", nbProcess);

                        throw new TryMultipleException(msgAbort, ex, nbProcess);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Contrôle la présence d'un paramètre IDbTransaction dans {action}
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="TryMultipleException">Lorsque {action} ne possède pas de paramètre de type IDbTransaction</exception>
        private void CheckIDbTransactionParameter(Delegate action)
        {
            Boolean isFind = false;
            if (ArrFunc.IsFilled(action.Method.GetParameters()))
                isFind = ArrFunc.Count(action.Method.GetParameters().Where(x => x.ParameterType.IsInterface && x.ParameterType.Equals(typeof(IDbTransaction))).ToArray()) == 1;

            if (false == isFind)
                throw new TryMultipleException($"{action.Method.Name} doesn't contains one IDbTransaction parameter");
        }


        /// <summary>
        /// Alimente le paramètre de type IDbTransaction avec {dbTransaction} 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="args"></param>
        /// <exception cref="TryMultipleException">Lorsque {action} ne possède pas de paramètre de type IDbTransaction ou si ce paramètre est alimenté</exception>
        /// <exception cref="ArgumentNullException">Lorsque args ou dbTransaction is null</exception>
        private static void ApplyDbTransaction(Delegate action, IDbTransaction dbTransaction, params object[] args)
        {
            if (null == args)
                throw new ArgumentNullException("args", "args is null");
            if (null == dbTransaction)
                throw new ArgumentNullException("dbTransaction", "dbTransaction is null");

            // La transaction dbTransaction est passée à la méthode si l'argument de type IDbTransaction et s'il est non renseigné
            for (int i = 0; i < ArrFunc.Count(action.Method.GetParameters()); i++)
            {
                Type itemType = action.Method.GetParameters()[i].ParameterType;
                if (itemType.IsInterface && itemType.Equals(typeof(IDbTransaction)))
                {
                    if (args.Length >= i + 1)
                    {
                        if (args[i] == null)
                            args[i] = dbTransaction;
                        else
                            throw new TryMultipleException("IDbTransaction parameter is not null. Null value is expected");
                    }
                    else
                    {
                        throw new TryMultipleException("args is incomplete");
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Ouverture d'une transaction
        /// </summary>
        private void BeginTran()
        {
            switch (TransactionType)
            {
                case TransactionTypeEnum.Internal:
                    dbTransaction = DataHelper.BeginTran(CS);
                    break;
                case TransactionTypeEnum.External:
                    if (null == BeginTranAction)
                        throw new NullReferenceException("BeginTranAction is null");

                    BeginTranAction.Invoke();
                    break;
            }
        }
        
        /// <summary>
        /// Validation d'une transaction
        /// </summary>
        private void CommitTran()
        {
            switch (TransactionType)
            {
                case TransactionTypeEnum.Internal:
                    DataHelper.CommitTran(dbTransaction);
                    break;
                case TransactionTypeEnum.External:
                    if (null == CommitTranAction)
                        throw new NullReferenceException("CommitTranAction is null");

                    CommitTranAction.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Rollback d'une transaction
        /// </summary>
        private void RollbackTran()
        {
            switch (TransactionType)
            {
                case TransactionTypeEnum.Internal:
                    DataHelper.RollbackTran(dbTransaction);
                    break;
                case TransactionTypeEnum.External:
                    if (null == RollbackTranAction)
                        throw new NullReferenceException("RollbackTranAction is null");

                    RollbackTranAction.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Vérifications avant ouverture d'une transaction
        /// </summary>
        /// <param name="action"></param>
        private void CheckTransactionalMode(Delegate action)
        {
            switch (TransactionType)
            {
                case TransactionTypeEnum.Internal:
                    CheckIDbTransactionParameter(action);
                    break;
                case TransactionTypeEnum.External:
                    if (null == BeginTranAction)
                        throw new NullReferenceException("BeginTranAction delagate is null. Call InitTransactionDelegate.");
                    if (null == CommitTranAction)
                        throw new NullReferenceException("CommitTranAction delagate is null. Call InitTransactionDelegate.");
                    if (null == RollbackTranAction)
                        throw new NullReferenceException("RollbackTranAction delagate is null. Call InitTransactionDelegate.");
                    break;
            }
        }

        /// <summary>
        ///  Permet de initialiser les delegués chargés d'ouvrir, fermer la transaction 
        /// </summary>
        /// <param name="beginTran"></param>
        /// <param name="commitTran"></param>
        /// <param name="rollbackTran"></param>
        public void InitTransactionDelegate(Action beginTran, Action commitTran, Action rollbackTran)
        {
            BeginTranAction = beginTran;
            CommitTranAction = commitTran;
            RollbackTranAction = rollbackTran;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsRetry"></param>
        /// FI 20211206 [XXXXX] Add
        public void InitIsRetryException(IsRetryException<Exception, String> pIsRetry)
        {
            _isRetry = pIsRetry;
        }

        /// <summary>
        ///  Retourne true si l'exception {ex} est un deadlock ou timeout sql
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// FI 20211206 [XXXXX] Add
        public Boolean IsRetryDefault(Exception ex, out string message)
        {
            Boolean ret = false;
            message = string.Empty;
            Exception exFirst = ExceptionTools.GetFirstRDBMSException(ex);
            if (null != exFirst)
            {
                SQLErrorEnum sqlErr = DataHelper.AnalyseSQLException(CS, exFirst);
                ret = ((sqlErr == SQLErrorEnum.DeadLock) || (sqlErr == SQLErrorEnum.Timeout));
                message = sqlErr.ToString();
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// FI 20211206 [XXXXX] Add
        private string SetMessageLog(string message)
        {
            string ret;

            if (StrFunc.IsFilled(LogHeader))
                ret = $"{LogHeader}{Cst.CrLf}{message}";
            else
                ret = message;
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20210121 [XXXXX] Add
    public class TryMultipleException : Exception
    {
        /// <summary>
        ///  Retourne true si l'exception a élé levée à l'intérieur de la méthode appelée
        /// </summary>
        public Boolean IsFromTargetInvocationException
        {
            get {
                return (this.InnerException != null) && this.InnerException.GetType().Equals(typeof(TargetInvocationException));
            }
        }

        /// <summary>
        /// Retourne l'exception levée à l'intérieur de la méthode appelée
        /// <para>Retourne null dans les autres cas</para>
        /// </summary>
        public Exception ActionException
        {
            get
            {
                if (IsFromTargetInvocationException)
                    return InnerException.InnerException;
                else
                    return null;
            }
        }

        public int AttemptNumber;

        public TryMultipleException() : base()
        { }
        public TryMultipleException(string message) : base(message)
        { }
        public TryMultipleException(string message, int attemptNumber) : base(message)
        {
            AttemptNumber = attemptNumber;
        }
        public TryMultipleException(string message, Exception exception) : base(message, exception)
        { }

        public TryMultipleException(string message, Exception exception,  int attemptNumber) : base(message, exception)
        {
            AttemptNumber = attemptNumber;
        }
    }
}
