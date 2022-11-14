using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Config
{
    public class ExperimentationRestApiOptions
    {
        private string _restAuthToken;
        public string ProjectId { get; set; }
        public long ExperimentID { get; set; }
        public string FlagKey { get; set; }
        public string Environment { get; set; }
        public int VersionId { get; set; }
        public string RestAuthToken
        {
            get => _restAuthToken;
            set
            {
                if (value.StartsWith("Bearer "))
                {
                    _restAuthToken = value;
                }
                else
                {
                    _restAuthToken = "Bearer " + value;
                }
            }
        }
    }
}
