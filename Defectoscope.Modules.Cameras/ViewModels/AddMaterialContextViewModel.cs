using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class AddMaterialContextViewModel : BindableBase, IDialogAware
    {
        public IBaslerRepository BaslerRepository { get; }
        private string _materialName;
        public string MaterialName
        {
            get { return _materialName; }
            set { SetProperty(ref _materialName, value); }
        }

        private DateTime _supplyTime;
        public DateTime SupplyTime
        {
            get { return _supplyTime; }
            set { SetProperty(ref _supplyTime, value); }
        }
        private DelegateCommand _addMaterialCommand;

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand AddMaterialCommand =>
            _addMaterialCommand ?? (_addMaterialCommand = new DelegateCommand(ExecuteAddMaterialCommand));

   

        public AddMaterialContextViewModel(IBaslerRepository baslerRepository)
        {
            BaslerRepository = baslerRepository;
        }

        private void ExecuteAddMaterialCommand()
        {
            BaslerRepository.MaterialModelCollection.Add(new MaterialModel
            {
                MaterialName = _materialName,
                SupplyTime= _supplyTime
            });
            ButtonResult result = ButtonResult.OK;
            //DialogParameters p = new DialogParameters();
            //p.Add("myParam", "The dialog closed by user");
            RequestClose?.Invoke(new DialogResult(result));
        }

        public string Title => "Материал";

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            ButtonResult result = ButtonResult.OK;
            RequestClose?.Invoke(new DialogResult(result));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
           
        }  
    }
}
