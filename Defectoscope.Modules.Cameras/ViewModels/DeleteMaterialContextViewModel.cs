using Kogerent.Services.Interfaces;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class DeleteMaterialContextViewModel : BindableBase, IDialogAware
    {
        private string _materialName;
        public string MaterialName
        {
            get { return _materialName; }
            set { SetProperty(ref _materialName, value); }
        }
        private DelegateCommand _deleteMaterialCommand;
        public DelegateCommand DeleteMaterialCommand =>
            _deleteMaterialCommand ?? (_deleteMaterialCommand = new DelegateCommand(ExecuteDeleteMaterialCommand));
        public IBaslerRepository BaslerRepository { get; }

        public DeleteMaterialContextViewModel(IBaslerRepository baslerRepository)
        {
            BaslerRepository = baslerRepository;
        }
        void ExecuteDeleteMaterialCommand()
        {
            BaslerRepository.MaterialModelCollection.Remove(BaslerRepository.CurrentMaterial);
            DialogResult result = new DialogResult();
            RequestClose?.Invoke(result);
        }

        public string Title => "";



        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            if (BaslerRepository.MaterialModelCollection.Count > 0)
            {
                BaslerRepository.CurrentMaterial = BaslerRepository.MaterialModelCollection[0];
            }

            DialogResult result = new DialogResult();
            RequestClose?.Invoke(result);
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            MaterialName = parameters.GetValue<string>("MaterialName");
        }
    }
}
