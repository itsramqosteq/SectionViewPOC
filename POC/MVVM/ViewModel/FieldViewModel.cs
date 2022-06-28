using POC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
   public class FieldViewModel 
    {
        private ImportExcelVM _ImportExcelVM;
        public ImportExcelVM ImportExcelVM
        {
            get { return _ImportExcelVM; }
        }
        private TextBoxVM _FirstName;

        public TextBoxVM FirstName
        {
            get { return _FirstName; }
        }
        private TextBoxVM _LastName;

        public TextBoxVM LastName
        {
            get { return _LastName; }
        }
        private ColorPickerVM _ColorPickerVM;

        public ColorPickerVM ColorPickerVM
        {
            get { return _ColorPickerVM; }
        }
        private MultiSelectVM _MultiSelectVM;

        public MultiSelectVM MultiSelectVM
        {
            get { return _MultiSelectVM; }
        }

       
        public FieldViewModel()
        {

            _ImportExcelVM = new ImportExcelVM();
            _FirstName = new TextBoxVM();
            _LastName = new TextBoxVM();
            _ColorPickerVM = new ColorPickerVM();
            _MultiSelectVM = new MultiSelectVM();
        }
    }
}
