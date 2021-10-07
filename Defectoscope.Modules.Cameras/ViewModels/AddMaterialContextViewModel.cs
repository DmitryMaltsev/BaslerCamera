using Kogerent.Services.Interfaces;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class AddMaterialContextViewModel : BindableBase
    {
        public IBaslerRepository BaslerRepository { get; }

        private DelegateCommand _executeAddMaterial;
        public DelegateCommand ExecuteAddMaterial =>
            _executeAddMaterial ?? (_executeAddMaterial = new DelegateCommand(ExecuteExecuteAddMaterial));

        public AddMaterialContextViewModel(IBaslerRepository baslerRepository)
        {
            BaslerRepository = baslerRepository;
        }

        void ExecuteExecuteAddMaterial()
        {

        }


    }
}
