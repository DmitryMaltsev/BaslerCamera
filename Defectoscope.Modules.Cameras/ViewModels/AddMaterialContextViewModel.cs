using Kogerent.Services.Interfaces;
using Kogerent.Utilities;

using LaserScan.Core.NetStandart.Models;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.IO;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class AddMaterialContextViewModel : BindableBase, IDialogAware
    {
        private string SettingsDir => Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Settings").FullName;
        public IBaslerRepository BaslerRepository { get; }
        public IXmlService XmlService { get; }

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

        public AddMaterialContextViewModel(IBaslerRepository baslerRepository, IXmlService xmlService)
        {
            BaslerRepository = baslerRepository;
            XmlService = xmlService;
            SupplyTime = DateTime.Now;
        }

        private void ExecuteAddMaterialCommand()
        {
            string filteredName = null;
            if (MaterialName != null)
            {
                char[] charsToTrim = { ' ' };
                filteredName = _materialName.Trim(charsToTrim);
            }

            if (!filteredName.IsNullOrEmpty())
            {
                BaslerRepository.MaterialModelCollection.Add(new MaterialModel
                {
                    MaterialName = filteredName,
                    SupplyTime = SupplyTime,
                    CameraDeltaList = new()

                });
                string path = Path.Combine(SettingsDir, "MaterialSettings.xml");
                XmlService.Write(path, BaslerRepository.MaterialModelCollection);
                ButtonResult result = ButtonResult.OK;
                RequestClose?.Invoke(new DialogResult(result));
            }
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
