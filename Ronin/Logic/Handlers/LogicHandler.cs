using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Annotations;
using Ronin.Data;
using Ronin.Protocols;

namespace Ronin.Logic.Handlers
{
    public abstract class LogicHandler : INotifyPropertyChanged
    {
        protected L2PlayerData _data;
        protected ActionsController _actionsController;

        [JsonIgnore]
        public bool Initialiased;

        public virtual void Init(L2PlayerData data, ActionsController actionsController)
        {
            _data = data;
            _actionsController = actionsController;
            Initialiased = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
