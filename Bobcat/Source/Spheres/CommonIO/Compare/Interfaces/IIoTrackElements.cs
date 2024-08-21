using System;

using EFS.SpheresIO;

namespace IOCompareCommon.Interfaces
{
    /// <summary>
    /// Decomposition element for a compare element. When a compare element implements the
    /// IIoTrackElements interface <seealso cref="IIoTrackElements"/>, the element will be decomposed in a number of 
    /// IoTrackElement objects.
    /// </summary>
    public class IoTrackElement
    {
        
        /// <summary>
        /// Id of the element
        /// </summary>
        /// <value>can be null</value>
        public object Id { get; private set; }

        /// <summary>
        /// match result
        /// </summary>
        public ValueErrorStatus ErrorStatus {get; private set; }

        /// <summary>
        /// result type name
        /// </summary>
        public string Type { get; private set; }

        public IoTrackElement(object pId, ValueErrorStatus pErrorStatus)
        {
            this.Id = pId;

            this.ErrorStatus = pErrorStatus;
        }

        public IoTrackElement(object pId, ValueErrorStatus pErrorStatus, string pType):
            this(pId, pErrorStatus)
        {
            this.Type = pType;
        }

    }

    /// <summary>
    /// Rule the way an IoTrackElement object is stocked. When implemented then any Key returned by GetKeyElements will produce a new data row,
    /// IOW a new line in the IOTRACKCOMPARE table.
    /// </summary>
    public interface IIoTrackElements
    {
        /// <summary>
        /// Keys vector, usually filled with all the compared identifier values
        /// </summary>
        /// <returns></returns>
        string[] GetKeyElements();
    }
}