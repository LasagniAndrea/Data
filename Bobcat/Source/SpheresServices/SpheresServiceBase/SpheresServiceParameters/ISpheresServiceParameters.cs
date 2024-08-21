using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;

namespace EFS.SpheresServiceParameters
{
    /// <summary>
    /// Spheres service interface containing the configuration procedure for a service instance
    /// </summary>
    public interface ISpheresServiceParameters
    {
        /// <summary>
        /// Define the needed service properties
        /// <list type="objet">
        /// <item>mandatory key : SERVICEENUM - type : Cst.ServiceEnum.SpheresGateBCS - description : service type</item>
        /// <item>optional key : ADDITIONALLOG - type : string - description : custom log output</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// the list can be enriched at pleasure
        /// </remarks>
        Dictionary<string, object> ServiceProperties
        {
            get;
        }

        /// <summary>
        /// Define the Form collection (BaseFormParameters based) which makes the user interface configuration
        /// </summary>
        /// <returns>The Form collection</returns>
        List<BaseFormParameters> InitModalConfiguration();

        /// <summary>
        /// Define the starting procedure for the user interface configuration
        /// </summary>
        /// <param name="formParameters">Form collection returned by InitModalConfiguration - mandatory</param>
        /// <returns>The parameters collection</returns>
        /// <remarks>
        /// Use a modal window open type
        /// </remarks>
        Dictionary<string, object> StartModalConfiguration(List<BaseFormParameters> formParameters);

        /// <summary>
        /// Save the install datas
        /// </summary>
        /// <param name="serviceName">Name of the service instance</param>
        /// <param name="serviceParameters">The parameters collection returned by StartModalConfiguration - mandatory</param>
        void WriteInstallInformation( string serviceName, Dictionary<string, object> serviceParameters);

        /// <summary>
        /// Delete the install datas
        /// </summary>
        /// <param name="serviceName"></param>
        void DeleteInstallInformation(string serviceName);

        /// <summary>
        /// Save the install datas on the execonfig file
        /// </summary>
        /// <param name="serviceFullPath"></param>
        /// <param name="parameters"></param>
        void UpdateSectionConfig(string serviceFullPath, StringDictionary parameters);

    }
}
