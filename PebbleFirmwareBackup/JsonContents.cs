using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PebbleFirmwareBackup
{
    [DataContract]
    class JsonContents
    {
        [DataMember]
        public JsonContentsFirmware recovery;
        [DataMember]
        public JsonContentsFirmware normal;
    }

    [DataContract]
    class JsonContentsFirmware
    {
        [DataMember]
        public string url;
        [DataMember]
        public long timestamp;
        [DataMember]
        public string notes;
        [DataMember]
        public string friendlyVersion;
        [DataMember]
        public string layouts;
    }
}
