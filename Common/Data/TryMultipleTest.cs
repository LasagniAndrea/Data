using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EFS.ApplicationBlocks.Data
{
    public partial class TryMultiple
    {
#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public static class Test
        {
            const string TradeIdentifier = "2107085765";
            const string CSQuality = @"Data Source=SVR-DB01\SQL2017;Initial Catalog=RD_QUALITY_M21_V10;Persist Security Info=False;User Id=sa;Password=efs98*;Workstation Id=127.0.0.1;Packet Size=4096";

            /// <summary>
            ///  Plusieurs exemples d'usage de TryMultiple  sur la base QUALITY
            /// </summary>
            public static void Run()
            {
                Run(CSQuality);
            }

            /// <summary>
            ///  Plusieurs exemples d'usage de TryMultiple 
            /// </summary>
            /// <param name="cs"></param>
            public static void Run(string cs)
            {

                System.Diagnostics.Debug.WriteLine($"Run tests on : {cs}");
                CS = cs;

                TryMultiple @try;

                #region pas de mode transactionnel
                //Exemple 1 : Appel d'une méthode avec 0 paramètre
                System.Diagnostics.Debug.WriteLine("Exemple1");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest")
                {
                    IsModeTransactional = false
                };
                @try.Exec(delegate { UpdateTradeExemple1(); });
                System.Diagnostics.Debug.WriteLine("Exemple1 successfully executed");
                #endregion

                #region Mode transactionnel interne
                //Exemple 2 : Appel d'une méthode avec 1 paramètre IDbTransaction
                //Une transaction est ouverte dans TryMultiple. Elle est utilisée lors de l'appel à la méthode 
                System.Diagnostics.Debug.WriteLine("Exemple2");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest")
                {
                    IsModeTransactional = true // Si true, la méthode doit avoir un paramètre IDbTransaction
                };
                @try.Exec<IDbTransaction>(delegate (IDbTransaction arg1) { UpdateTradeExemple2(arg1); }, null);
                System.Diagnostics.Debug.WriteLine("Exemple2 successfully executed");

                //Exemple 3 : Appel d'une méthode avec 2 paramètres (IDbTransaction et String). 
                //Une transaction est ouverte dans TryMultiple. Elle est utilisée lors de l'appel à la méthode 
                System.Diagnostics.Debug.WriteLine("Exemple3");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest")
                {
                    IsModeTransactional = true // Si true, la méthode doit avoir un paramètre IDbTransaction
                };
                @try.Exec<IDbTransaction, String>(delegate (IDbTransaction arg1, String arg2) { UpdateTradeExemple3(arg1, arg2); }, null, TradeIdentifier);
                System.Diagnostics.Debug.WriteLine("Exemple3 successfully executed");


                //Exemple 4 : Appel d'une méthode avec 2 paramètres (IDbTransaction et String). 
                //Une transaction est ouverte dans TryMultiple. Elle est utilisée lors de l'appel à la méthode 
                //Récupération de la valeur retour de type  Cst.ErrLevel
                System.Diagnostics.Debug.WriteLine("Exemple4");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest")
                {
                    IsModeTransactional = true
                };
                object ret = @try.Exec<IDbTransaction, String, Cst.ErrLevel>(delegate (IDbTransaction arg1, string arg2) { return UpdateTradeExemple4(arg1, arg2); }, null, TradeIdentifier);
                System.Diagnostics.Debug.WriteLine("Exemple4 successfully executed");
                #endregion

                #region Mode transactionnel externe
                //Exemple 5 : Appel d'une méthode sans paramètre
                //Une transaction est ouverte à l'extérieur de TryMultiple via des delegue
                System.Diagnostics.Debug.WriteLine("Exemple5");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest")
                {
                    IsModeTransactional = true,
                    TransactionType = TransactionTypeEnum.External
                };
                @try.InitTransactionDelegate(delegate { BeginTran(); }, delegate { CommitTran(); }, delegate { RollbackTran(); });
                @try.Exec(delegate () { UpdateTradeExemple1(); });
                System.Diagnostics.Debug.WriteLine("Exemple5 successfully executed");

                //Exemple 6 : Appel d'une méthode avec 1 paramètre (String). 
                //Une transaction est ouverte à l'extérieur de TryMultiple via des delegue
                //Récupération de la valeur retour de type  Cst.ErrLevel
                System.Diagnostics.Debug.WriteLine("Exemple6");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest")
                {
                    IsModeTransactional = true,
                    TransactionType = TransactionTypeEnum.External
                };
                @try.InitTransactionDelegate(delegate { BeginTran(); }, delegate { CommitTran(); }, delegate { RollbackTran(); });
                ret = @try.Exec<String, Cst.ErrLevel>(delegate (String arg1) { return UpdateTradeExemple5(arg1); }, TradeIdentifier);
                System.Diagnostics.Debug.WriteLine("Exemple6 successfully executed");
                #endregion

                #region Exemple gestion d'une exception particulière
                //Exemple 7 : Gestion d'une exception particuculière
                System.Diagnostics.Debug.WriteLine("Exemple7");
                @try = new TryMultiple(CS, "AppSettingKeyTest", "DescTest");
                @try.InitIsRetryException(IsRetryException);
                @try.Exec(delegate { UpdateTradeExemple1(); });
                System.Diagnostics.Debug.WriteLine("Exemple7 successfully executed");
                #endregion

            }


            private static void UpdateTradeExemple1()
            {
                UpdateTrade(CS, null, TradeIdentifier);
            }

            private static void UpdateTradeExemple2(IDbTransaction dbTransaction)
            {
                UpdateTrade(null, dbTransaction, TradeIdentifier);
            }

            private static void UpdateTradeExemple3(IDbTransaction dbTransaction, string pIdentifier)
            {
                UpdateTrade(null, dbTransaction, pIdentifier);
            }

            private static Cst.ErrLevel UpdateTradeExemple4(IDbTransaction dbTransaction, string pIdentifier)
            {
                return UpdateTrade(null, dbTransaction, pIdentifier);
            }

            private static Cst.ErrLevel UpdateTradeExemple5(string pIdentifier)
            {
                return UpdateTrade(null, DbTransaction, pIdentifier);
            }


            private static Cst.ErrLevel UpdateTrade(string cs, IDbTransaction dbTransaction, string pIdentifier)
            {
                string sql = $"update TRADE set EXTLLINK = isnull(EXTLLINK,'') + 'TEST' where IDENTIFIER = '{pIdentifier}'";
                DataHelper.ExecuteNonQuery(cs, dbTransaction, CommandType.Text, sql);
                return Cst.ErrLevel.SUCCESS;
            }


            private static Boolean IsRetryException(Exception ex, out string message)
            {
                message = string.Empty;
                bool ret = false;

                if (ex.GetType().Equals(typeof(NullReferenceException)))
                    ret = true;

                return ret;
            }


            static IDbTransaction DbTransaction;
            static string CS;

            private static void BeginTran()
            {
                DbTransaction = DataHelper.BeginTran(CS);
            }
            private static void CommitTran()
            {
                 DataHelper.CommitTran(DbTransaction);
            }

            private static void RollbackTran()
            {
                DataHelper.RollbackTran(DbTransaction);
            }

        }
#endif
    }
}
