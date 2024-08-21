using System;
using System.Threading;
using System.Diagnostics;
using System.Globalization;  

using EFS.ACommon; 
using EFS.Common.Log;

namespace EFS.Common
{
    /// <summary>
    /// Classe de base pour pour exécution d'une tâche dans un thread indépendant 
    /// </summary>
    /// FI 20140519 [19923] add class
    public abstract class ThreadTaskBase
    {
        public RegisteredWaitHandle Handle = null;
        public string OtherInfo = "default";

        /// <summary>
        /// Cuture dans laquelle doit s'exécuter le thread
        /// </summary>
        public string culture;
        /// <summary>
        ///  Session
        /// </summary>
        public AppSession Session;
        /// <summary>
        /// URL si Application Spheres® web 
        /// </summary>
        public string URL;
        /// <summary>
        /// Gestionnaire d'erreur pour notification en cas d'erreur durant l'exécution de la tâche
        /// </summary>
        public ErrorManager errManager;
                    
        /// <summary>
        /// Tâche à exécuter 
        /// </summary>
        public virtual void ExecuteTask()
        {
            throw new Exception("ExecuteTask must be override"); 
        }
    }

    /// <summary>
    ///  multi-Threading tools
    /// </summary>
    /// FI 20140519 [19923] add class
    public class ThreadTools : ThreadingTools
    {
        /// <summary>
        /// Exécution de la méthode ExecuteTask présente dans {taskInfo} dans un Thread indépendant
        /// <see cref="http://msdn.microsoft.com/en-us/library/system.threading.registeredwaithandle.unregister.aspx"/>
        /// </summary>
        /// <param name="taskInfo"></param>
        public static void ExecuteTask(ThreadTaskBase taskInfo)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            // The TaskInfo for the task includes the registered wait 
            // handle returned by RegisterWaitForSingleObject.  This 
            // allows the wait to be terminated when the object has 
            // been signaled once (see WaitProc).

            taskInfo.Handle = ThreadPool.RegisterWaitForSingleObject(autoResetEvent,
                new WaitOrTimerCallback(WaitProc), taskInfo, 1000, false);


            // The main thread waits 1 seconds, to demonstrate the 
            // time-outs on the queued thread, and then signals.
            Thread.Sleep(1000);
            autoResetEvent.Set();

        }
        /// <param name="stateObject"></param>
        /// <param name="timedOut"></param>
        /// EG 20160404 Migration vs2013
        /// FI 20170215 [XXXXX] Modify
        private static void WaitProc(object stateObject, bool pTimeOut)
        {
            ThreadTaskBase taskInfo = (ThreadTaskBase)stateObject;
            // EG 20160404 Migration vs2013
            //string cause = "TIMED OUT";
            try
            {
                if (false == pTimeOut)
                {

                    if (StrFunc.IsFilled(taskInfo.culture))
                    {
                        // FI 20170215 [XXXXX] Appel ThreadTools.SetCurrentCulture
                        //SystemTools.SetCurrentCulture(taskInfo.culture);
                        ThreadTools.SetCurrentCulture(taskInfo.culture);
                    }
                    // EG 20160404 Migration vs2013
                    //cause = "SIGNALED";
                    taskInfo.ExecuteTask();
                }
            }
            catch (Exception ex)
            {
                // EG 20160404 Migration vs2013
                // Ecriture de l'exception dans les journeaux de log
                ErrorBlock error = new ErrorBlock(ex, taskInfo.Session, taskInfo.URL);
                ErrorManager errManager = taskInfo.errManager;
                errManager.Write(error);

                //Mort du Thread
                Thread.CurrentThread.Abort();
            }
            finally
            {
                if ((false == pTimeOut) && (null != taskInfo))
                {
                    // If the callback method executes because the WaitHandle is 
                    // signaled, stop future execution of the callback method 
                    // by unregistering the WaitHandle. 
                    if (null != taskInfo.Handle)
                        taskInfo.Handle.Unregister(null);
                }
            }
        }
    }
}