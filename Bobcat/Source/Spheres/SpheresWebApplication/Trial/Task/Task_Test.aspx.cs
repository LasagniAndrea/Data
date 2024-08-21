using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;



namespace EFS.Spheres.Trial
{
    public partial class Task_Test : System.Web.UI.Page
    {
        private int nbInsert = 0;
        private int sleepTime = 0;
        private string cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (StrFunc.IsEmpty(txtNbInsert.Text))
                txtNbInsert.Text = "20";

            if (StrFunc.IsEmpty(txtSleepTime.Text))
                txtSleepTime.Text = "1";

            if (StrFunc.IsEmpty(txtCS.Text))
                txtCS.Text = SessionTools.CS;

            nbInsert = IntFunc.IntValue(txtNbInsert.Text);
            sleepTime = IntFunc.IntValue(txtSleepTime.Text);
            cs = txtCS.Text;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button1_Click(object sender, EventArgs e)
        {
            // Insert en parallèle sur 3 tables dans 1 même transaction
            
            IDbTransaction dbtransac = null;
            if (StrFunc.IsEmpty(cs))
                throw new Exception("connectionString is empty");
            
            if (chkTransaction.Checked)
                dbtransac = DataHelper.BeginTran(cs);

            List<Task> lstTask = new List<Task>
            {
                Task.Run(() => SetTableTEST(cs, dbtransac, "TEST1", nbInsert, sleepTime)),
                Task.Run(() => SetTableTEST(cs, dbtransac, "TEST2", nbInsert, sleepTime)),
                Task.Run(() => SetTableTEST(cs, dbtransac, "TEST3", nbInsert, sleepTime))
            };


            try
            {
                Task.WaitAll(lstTask.ToArray());
                if (null != dbtransac)
                    DataHelper.CommitTran(dbtransac);
            }
            catch (AggregateException ae)
            {
                if (null != dbtransac && (null != dbtransac.Connection))
                    DataHelper.RollbackTran(dbtransac);
                throw ae.Flatten();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button2_Click(object sender, EventArgs e)
        {
            // usage d'une TransactionScope pour l'ensemble des task
            
            if (StrFunc.IsEmpty(cs))
                throw new Exception("connectionString is empty");


            // Insert en parallèle sur 3 tables dans 1 même transaction            
            using (var scope = new TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
            {

                List<Task> lstTask = new List<Task>
                {
                    Task.Run(() => SetTableTEST(cs, null, "TEST1", nbInsert, sleepTime)),
                    Task.Run(() => SetTableTEST(cs, null, "TEST2", nbInsert, sleepTime)),
                    Task.Run(() => SetTableTEST(cs, null, "TEST3", nbInsert, sleepTime))
                };

                try
                {
                     Task.WaitAll(lstTask.ToArray());
                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten();
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button3_Click(object sender, EventArgs e)
        {
            // usage de DependentTransaction
            // Pour chaque task on passe une DependentTransaction

            if (StrFunc.IsEmpty(cs))
                throw new Exception("connectionString is empty");

            // Insert en parallèle sur 3 tables dans 1 même transaction            
            using (var scope = new TransactionScope( TransactionScopeAsyncFlowOption.Enabled ))
            {
                DependentTransaction dtr1 = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                DependentTransaction dtr2 = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                DependentTransaction dtr3 = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);

                List<Task> lstTask = new List<Task>
                {
                    Task.Run(() => SetTableTEST2(cs, dtr1, "TEST1", nbInsert, sleepTime)),
                    Task.Run(() => SetTableTEST2(cs, dtr2, "TEST2", nbInsert, sleepTime)),
                    Task.Run(() => SetTableTEST2(cs, dtr3, "TEST3", nbInsert, sleepTime))
                };
                try
                {
                    Task.WaitAll(lstTask.ToArray());
                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten();
                }
                scope.Complete(); 
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button4_Click(object sender, EventArgs e)
        {
            //Exemple basique sur l'usage de TransactionScope
            //=> Sans faire de beginTran on est en mode transactionnel

            if (StrFunc.IsEmpty(cs))
                throw new Exception("connectionString is empty");

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                SetTableTEST(cs, null, "TEST1", nbInsert, sleepTime);
                scope.Complete(); 
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTansaction"></param>
        /// <param name="pTableName"></param>
        /// <param name="pNbItem"></param>
        /// <param name="pSleep"></param>
        private static void SetTableTEST(string pCs, IDbTransaction pDbTansaction, string pTableName, int pNbItem, int pSleep)
        {
            for (int i = 0; i < pNbItem; i++)
            {
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), i);
                dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DT), DateTime.Now);

                string query = StrFunc.AppendFormat("insert into dbo.{0}(ID,DT) values (@ID,@DT)", pTableName);
                DataHelper.ExecuteNonQuery(pCs, pDbTansaction, CommandType.Text, query, dp.GetArrayDbParameter());

                if (pSleep > 0)
                    Thread.Sleep(pSleep * 1000);

                //if ((pTableName == "TEST2") && (20 == i))
                //    DataHelper.RollbackTran(pDbTansaction);  
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTansaction"></param>
        /// <param name="pTableName"></param>
        /// <param name="pNbItem"></param>
        /// <param name="pSleep"></param>
        private async static Task SetTableTESTAsync(string pCs, IDbTransaction pDbTansaction, string pTableName, int pNbItem, int pSleep)
        {
            await Task.Run(() => SetTableTEST(pCs, pDbTansaction, pTableName, pNbItem, pSleep));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDependentTransaction"></param>
        /// <param name="pTableName"></param>
        /// <param name="pNbItem"></param>
        /// <param name="pSleep"></param>
        private static void SetTableTEST2(string pCs, DependentTransaction pDependentTransaction, string pTableName, int pNbItem, int pSleep)
        {
            //try
            //{
                using (TransactionScope ts = new TransactionScope(pDependentTransaction))
                {
                    for (int i = 0; i < pNbItem; i++)
                    {
                        DataParameters dp = new DataParameters();
                        dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), i);
                        dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DT), DateTime.Now);

                        string query = StrFunc.AppendFormat("insert into dbo.{0}(ID,DT) values (@ID,@DT)", pTableName);
                        DataHelper.ExecuteNonQuery(pCs, null, CommandType.Text, query, dp.GetArrayDbParameter());

                        if (pSleep > 0)
                            Thread.Sleep(pSleep * 1000);

                        //if ((pTableName == "TEST2") && (20 == i))
                        //    DataHelper.RollbackTran(pDbTansaction);  
                    }
                    ts.Complete();
                }
            //}
            //finally
            //{
            //    pDependentTransaction.Complete();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTansaction"></param>
        /// <param name="pTableName"></param>
        /// <param name="pNbItem"></param>
        /// <param name="pSleep"></param>
        /// <returns></returns>
        private async static Task SetTableTEST2Async(string pCs, DependentTransaction pDbTansaction, string pTableName, int pNbItem, int pSleep)
        {
            await Task.Run(() => SetTableTEST2(pCs, pDbTansaction, pTableName, pNbItem, pSleep));
        }

    }
}