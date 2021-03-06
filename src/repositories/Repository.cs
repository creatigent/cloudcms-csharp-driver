using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CloudCMS;

namespace CloudCMS
{
    class Repository : AbstractDocument,
                        IRepository
    {
        public string PlatformId { get; }

        public Repository(ICloudCMSDriver driver, JObject obj) : base(driver, obj)
        {
            this.PlatformId = obj.SelectToken("platformId").ToString();
        }

        public override string URI
        {
            get 
            {
                return "/repositories/" + Id;
            }
        }

        public async Task<List<IBranch>> ListBranchesAsync()
        {
            string uri = this.URI + "/branches";
            JObject response = await Driver.GetAsync(uri);

            List<IBranch> branches = new List<IBranch>();
            JArray rows = (JArray) response.SelectToken("rows");
            foreach(var row in rows)
            {
                JObject branchObj = (JObject) row;
                IBranch branch = new Branch(this, branchObj);
                branches.Add(branch);
            }

            return branches;
        }

        public async Task<IBranch> ReadBranchAsync(string branchId)
        {
            string uri = this.URI + "/branches/" + branchId;
            IBranch branch = null;
            try
            {
                JObject response = await Driver.GetAsync(uri);
                branch = new Branch(this, response);
            }
            catch (CloudCMSRequestException)
            {
                branch = null;
            }
            return branch;
        }

        public Task<IBranch> MasterAsync()
        {
            return ReadBranchAsync("master");
        }


        public override string TypeId
        {
            get
            {
                return "repository";
            }
        }

        public override Reference Ref
        {
            get
            {
                return Reference.create(TypeId, PlatformId, Id);
            }
        }
    }
}