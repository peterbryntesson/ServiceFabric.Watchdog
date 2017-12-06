using System.Fabric;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public class SFNode : SFBase
    {
        private SFRoot _parent;
        private Node _node;

        public SFRoot Parent { get { return _parent; } }
        public Node Properties { get { return _node; } }
        public NodeLoadInformation LoadInformation { get; private set; }


        public SFNode(FabricClient fabricClient, SFRoot parent, Node node)
            : base(fabricClient)
        {
            _parent = parent;
            _node = node;
        }

        protected override async Task OnEnumChildren(bool deep)
        {
            LoadInformation = await _fabricClient.QueryManager.GetNodeLoadInformationAsync(_node.NodeName);
        }
    }
}
