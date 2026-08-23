using System.Windows;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>供应商编辑弹窗的 ViewModel，草稿模式同商品编辑。</summary>
public class SupplierEditViewModel : ViewModelBase
{
    private readonly bool _isEdit;

    public SupplierEditViewModel(Supplier? supplier)
    {
        _isEdit = supplier != null;
        if (supplier != null)
        {
            _name = supplier.Name;
            _contactPerson = supplier.ContactPerson;
            _phone = supplier.Phone;
            _address = supplier.Address;
        }
    }

    public string Title => _isEdit ? "编辑供应商" : "新增供应商";

    private string _name = "";
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _contactPerson = "";
    public string ContactPerson
    {
        get => _contactPerson;
        set => SetProperty(ref _contactPerson, value);
    }

    private string _phone = "";
    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    private string _address = "";
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("供应商名称不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    public void ApplyTo(Supplier supplier)
    {
        supplier.Name = Name.Trim();
        supplier.ContactPerson = ContactPerson.Trim();
        supplier.Phone = Phone.Trim();
        supplier.Address = Address.Trim();
    }
}
