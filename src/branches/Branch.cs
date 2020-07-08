using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CloudCMS;
using System.Net.Http;

namespace CloudCMS
{
    public class Branch : AbstractRepositoryDocument,
                    IBranch
    {
        public Branch(IRepository repository, JObject obj) : base(repository, obj)
        {

        }

        public override string URI
        {
            get
            {
                return Repository.URI + "/branches/" + this.Id; 
            }
        }

        public override string TypeId
        {
            get
            {
                return "branch";
            }
        }

        public override Reference Ref
        {
            get
            {
                return Reference.create(TypeId, Repository.PlatformId, RepositoryId, Id);
            }
        }

        public bool IsMaster()
        {
            return Data["type"].ToString().Equals("MASTER");
        }

        public async Task<IBaseNode> ReadNodeAsync(string nodeId)
        {
            string uri = this.URI + "/nodes/" + nodeId;
            IBaseNode node = null;
            try
            {
                JObject response = await Driver.GetAsync(uri);
                node = new Node(this, response);
            }
            catch (CloudCMSRequestException)
            {
                node = null;
            }

            return node;
        }

        public async Task<List<IBaseNode>> QueryNodesAsync(JObject query, JObject pagination = null)
        {
            string uri = this.URI + "/nodes/query";
            
            IDictionary<string, string> queryParams = null;
            if (pagination != null)
            {
                queryParams = pagination.ToObject<Dictionary<string, string>>();
            }

            HttpContent content = new StringContent(query.ToString());

            JObject response = await Driver.PostAsync(uri, queryParams, content);

            JArray nodeArray = (JArray) response.SelectToken("rows");
            List<IBaseNode> nodes = NodeUtil.NodeList(nodeArray, this);
            return nodes;
        }

        public async Task<List<IBaseNode>> FindNodesAsync(JObject config, JObject pagination = null)
        {
            string uri = this.URI + "/nodes/find";

            IDictionary<string, string> queryParams = null;
            if (pagination != null)
            {
                queryParams = pagination.ToObject<Dictionary<string, string>>();
            }

            HttpContent content = new StringContent(config.ToString());

            JObject response = await Driver.PostAsync(uri, queryParams, content);

            JArray nodeArray = (JArray) response.SelectToken("rows");
            List<IBaseNode> nodes = NodeUtil.NodeList(nodeArray, this);
            return nodes;         
        }

        public Task<IBaseNode> CreateNodeAsync()
        {
            return CreateNodeAsync(new JObject());
        }

        public async Task<IBaseNode> CreateNodeAsync(JObject nodeObj, JObject options = null)
        {
            string uri = this.URI + "/nodes";

            IDictionary<string, string> queryParams = new Dictionary<string, string>();
            
            if (options != null)
            {
                IDictionary<string, string> optionsDict = options.ToObject<Dictionary<string, string>>();

                if (optionsDict.ContainsKey("rootNodeId"))
                {
                    queryParams.Add("rootNodeId", optionsDict["rootNodeId"]);
                }
                else
                {
                    queryParams.Add("rootNodeId", "root");
                }

                if (optionsDict.ContainsKey("associationType"))
                {
                    queryParams.Add("associationType", optionsDict["associationType"]);
                }
                else
                {
                    queryParams.Add("associationType", "a:child");
                }

                if (optionsDict.ContainsKey("parentFolderPath"))
                {
                    queryParams.Add("parentFolderPath", optionsDict["parentFolderPath"]);
                }
                else if (optionsDict.ContainsKey("folderPath"))
                {
                    queryParams.Add("parentFolderPath", optionsDict["folderPath"]);
                }
                else if (optionsDict.ContainsKey("folderpath"))
                {
                    queryParams.Add("parentFolderPath", optionsDict["folderpath"]);
                }

                if (optionsDict.ContainsKey("filePath"))
                {
                    queryParams.Add("filePath", optionsDict["filePath"]);
                }
                else if (optionsDict.ContainsKey("filepath"))
                {
                    queryParams.Add("filePath", optionsDict["filepath"]);
                }
            }

            HttpContent content = new StringContent(nodeObj.ToString());

            JObject response = await Driver.PostAsync(uri, queryParams, content);
            string nodeId = response.SelectToken("_doc").ToString();

            return await ReadNodeAsync(nodeId);
        }

        public async Task<INode> RootNodeAsync()
        {
            return (INode) await ReadNodeAsync("root");
        }
    }
}