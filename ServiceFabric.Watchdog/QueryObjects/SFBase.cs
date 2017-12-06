using System.Fabric;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public abstract class SFBase
    {
        protected FabricClient _fabricClient;
        private bool _fetchedChildren;

        public SFBase(FabricClient fabricClient)
        {
            _fabricClient = fabricClient;
        }

        public async Task EnumChildren( bool deep = false)
        {
            lock (this)
            {
                if (_fetchedChildren)
                    return;
            }

            await OnEnumChildren(deep);

            lock (this)
            {
                _fetchedChildren = true;
            }
        }

        public async Task Refresh(bool deep = false)
        {
            lock (this)
            {
                _fetchedChildren = false;
            }
            await EnumChildren(deep);
        }

        protected abstract Task OnEnumChildren(bool deep);

    }
}