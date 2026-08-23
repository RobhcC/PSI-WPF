using System.Windows;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>客户编辑弹窗的 ViewModel，草稿模式同商品/供应商编辑。</summary>
public class CustomerEditViewModel : ViewModelBase
{
    private readonly bool _isEdit;

    public CustomerEditViewModel(Customer? customer)
    {
        _isEdit = customer != null;
        if (customer != null)
        {
            _name = customer.Name;
            _contactPerson = customer.ContactPerson;
            _phone = customer.Phone;
            _address = customer.Address;
        }
    }

    public string Title => _isEdit ? "编辑客户" : "新增客户";

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
            MessageBox.Show("客户名称不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    public void ApplyTo(Customer customer)
    {
        customer.Name = Name.Trim();
        customer.ContactPerson = ContactPerson.Trim();
        customer.Phone = Phone.Trim();
        customer.Address = Address.Trim();
    }
}
