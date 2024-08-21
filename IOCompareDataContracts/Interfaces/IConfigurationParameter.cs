using IOCompareCommon.DataContracts;

namespace IOCompareCommon.Interfaces
{
    /// <summary>
    /// Interface that should be implemented by any TypedIOTaskParam sub-class whose relative value must be inserted inside of an SQL request.
    /// </summary>
    public interface IConfigurationParameter
    {
        ConfigurationParameter ConfigurationParameter
        { get; set; }

    }
}
