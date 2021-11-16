using AutoLotModel;
using System.Data;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System;

namespace Vadean_Flaviu_Lab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    enum ActionState
    {
        New,
        Edit,
        Delete,
        Nothing
    }

    public partial class MainWindow : Window
    {
        ActionState action = ActionState.Nothing;

        AutoLotEntitiesModel ctx = new AutoLotEntitiesModel();
        CollectionViewSource customerViewSource;
        CollectionViewSource inventoryViewSource;
        CollectionViewSource customerOrdersViewSource;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            customerViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customerViewSource")));
            customerViewSource.Source = ctx.Customers.Local;
            ctx.Customers.Load();

            inventoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("inventoryViewSource")));
            inventoryViewSource.Source = ctx.Inventories.Local;
            ctx.Inventories.Load();

            customerOrdersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customerOrdersViewSource")));
            //customerOrdersViewSource.Source = ctx.Orders.Local;
            ctx.Orders.Load();

            cmbCustomers.ItemsSource = ctx.Customers.Local;
            cmbCustomers.SelectedValuePath = "CustId";

            cmbInventory.ItemsSource = ctx.Inventories.Local;
            cmbInventory.SelectedValuePath = "CarId";

            EnableElementsOnCustomersUI(action);
            EnableElementsOnInventoryUI(action);
            EnableElementsOnOrdersUI(action);

            BindDataGrid();
        }

        private void TabItemChanged_Click(object sender, SelectionChangedEventArgs e)
        {
            var tabController = e.OriginalSource as TabControl;

            if(tabController == null)
            {
                return;
            }

            action = ActionState.Nothing;

            EnableElementsOnCustomersUI(action);
            EnableElementsOnInventoryUI(action);
            EnableElementsOnOrdersUI(action);
        }

        private void BindDataGrid()
        {
            var queryOrder = from ord in ctx.Orders
                             join cust in ctx.Customers on ord.CustId equals
                             cust.CustId
                             join inv in ctx.Inventories on ord.CarId
                 equals inv.CarId
                             select new
                             {
                                 ord.OrderId,
                                 ord.CarId,
                                 ord.CustId,
                                 cust.FirstName,
                                 cust.LastName,
                                 inv.Make,
                                 inv.Color
                             };
            customerOrdersViewSource.Source = queryOrder.ToList();
        }

        #region Customers Menu
        private void SetValidationBinding()
        {
            Binding firstNameValidationBinding = new Binding();
            firstNameValidationBinding.Source = customerViewSource;
            firstNameValidationBinding.Path = new PropertyPath("FirstName");
            firstNameValidationBinding.NotifyOnValidationError = true;
            firstNameValidationBinding.Mode = BindingMode.TwoWay;
            firstNameValidationBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //string required
            firstNameValidationBinding.ValidationRules.Add(new StringNotEmpty());
            firstNameTextBox.SetBinding(TextBox.TextProperty, firstNameValidationBinding);
            Binding lastNameValidationBinding = new Binding();
            lastNameValidationBinding.Source = customerViewSource;
            lastNameValidationBinding.Path = new PropertyPath("LastName");
            lastNameValidationBinding.NotifyOnValidationError = true;
            lastNameValidationBinding.Mode = BindingMode.TwoWay;
            lastNameValidationBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //string min length validator
            lastNameValidationBinding.ValidationRules.Add(new StringMinLengthValidator());
            lastNameTextBox.SetBinding(TextBox.TextProperty, lastNameValidationBinding); //setare binding nou
        }
        void EnableElementsOnCustomersUI(ActionState state)
        {
            btnNew.IsEnabled = false;
            btnEdit.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnPrev.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnSave.IsEnabled = false;
            btnCancel.IsEnabled = false;
            firstNameTextBox.IsEnabled = false;
            lastNameTextBox.IsEnabled = false;
            customerDataGrid.IsEnabled = false;

            switch (state)
            {
                case ActionState.New:
                    btnSave.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    firstNameTextBox.IsEnabled = true;
                    lastNameTextBox.IsEnabled = true;
                    firstNameTextBox.Text = "";
                    lastNameTextBox.Text = "";
                    break;
                case ActionState.Edit:
                    btnSave.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    firstNameTextBox.IsEnabled = true;
                    lastNameTextBox.IsEnabled = true;
                    break;
                case ActionState.Delete:
                    btnSave.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    break;
                case ActionState.Nothing:
                    btnNew.IsEnabled = true;
                    btnEdit.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    btnPrev.IsEnabled = true;
                    btnNext.IsEnabled = true;
                    customerDataGrid.IsEnabled = true;
                    customerDataGrid.UnselectAll();
                    firstNameTextBox.Text = "";
                    lastNameTextBox.Text = "";
                    break;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Customer customer = null;
            SetValidationBinding();
            if (action == ActionState.New)
            {
                try
                {
                    //instantiem Customer entity
                    customer = new Customer()
                    {
                        FirstName = firstNameTextBox.Text.Trim(),
                        LastName = lastNameTextBox.Text.Trim()
                    };
                    if (string.IsNullOrEmpty(firstNameTextBox.Text) == false && string.IsNullOrEmpty(lastNameTextBox.Text) == false)
                    {
                        //adaugam entitatea nou creata in context
                        ctx.Customers.Add(customer);
                        customerViewSource.View.Refresh();
                        //salvam modificarile
                        ctx.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("Completati toate campurile!");
                    }
                }
                //using System.Data;
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (action == ActionState.Edit)
            {
                try
                {
                    if (string.IsNullOrEmpty(firstNameTextBox.Text) == false && string.IsNullOrEmpty(lastNameTextBox.Text) == false)
                    {
                        customer = (Customer)customerDataGrid.SelectedItem;
                        customer.FirstName = firstNameTextBox.Text.Trim();
                        customer.LastName = lastNameTextBox.Text.Trim();
                        //salvam modificarile
                        ctx.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("Completati toate campurile!");
                    }
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                customerViewSource.View.Refresh();
                // pozitionarea pe item-ul curent
                customerViewSource.View.MoveCurrentTo(customer);
            }
            else if (action == ActionState.Delete)
            {
                try
                {
                    customer = (Customer)customerDataGrid.SelectedItem;
                    ctx.Customers.Remove(customer);
                    ctx.SaveChanges();
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                customerViewSource.View.Refresh();
            }

            action = ActionState.Nothing;
            EnableElementsOnCustomersUI(action);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.New;
            EnableElementsOnCustomersUI(action);
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(firstNameTextBox.Text) == false && string.IsNullOrEmpty(lastNameTextBox.Text) == false)
            //{
                action = ActionState.Edit;
                EnableElementsOnCustomersUI(action);
                SetValidationBinding();
            //}
            //else
            //{
            //    MessageBox.Show("Nu ati selectat o inregistrare pe care sa o editati!");
            //}
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(firstNameTextBox.Text) == false && string.IsNullOrEmpty(lastNameTextBox.Text) == false)
            {
                action = ActionState.Delete;
                EnableElementsOnCustomersUI(action);
            }
            else
            {
                MessageBox.Show("Nu ati selectat o inregistrare pe care sa o stergeti!");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.Nothing;
            EnableElementsOnCustomersUI(action);
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            customerViewSource.View.MoveCurrentToPrevious();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            customerViewSource.View.MoveCurrentToNext();
        }

        #endregion
        #region Inventory Menu
        private void btnNewInventory_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.New;
            EnableElementsOnInventoryUI(action);
        }

        private void btnEditInventory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(colorTextBox.Text) == false && string.IsNullOrEmpty(makeTextBox.Text) == false)
            {
                action = ActionState.Edit;
                EnableElementsOnInventoryUI(action);
            }
            else
            {
                MessageBox.Show("Nu ati selectat o inregistrare pe care sa o editati!");
            }
        }

        private void btnDeleteInventory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(colorTextBox.Text) == false && string.IsNullOrEmpty(makeTextBox.Text) == false)
            {
                action = ActionState.Delete;
                EnableElementsOnInventoryUI(action);
            }
            else
            {
                MessageBox.Show("Nu ati selectat o inregistrare pe care sa o stergeti!");
            }
        }

        private void btnSaveInventory_Save(object sender, RoutedEventArgs e)
        {
            Inventory inventory = null;
            if (action == ActionState.New)
            {
                try
                {
                    //instantiem Inventory entity
                    inventory = new Inventory()
                    {
                        Color = colorTextBox.Text.Trim(),
                        Make = makeTextBox.Text.Trim()
                    };
                    if (string.IsNullOrEmpty(colorTextBox.Text) == false && string.IsNullOrEmpty(makeTextBox.Text) == false)
                    {
                        //adaugam entitatea nou creata in context
                        ctx.Inventories.Add(inventory);
                        inventoryViewSource.View.Refresh();
                        //salvam modificarile
                        ctx.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("Completati toate campurile!");
                    }
                }
                //using System.Data;
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (action == ActionState.Edit)
            {
                try
                {
                    if (string.IsNullOrEmpty(colorTextBox.Text) == false && string.IsNullOrEmpty(makeTextBox.Text) == false)
                    {
                        inventory = (Inventory)inventoryDataGrid.SelectedItem;
                        inventory.Color = colorTextBox.Text.Trim();
                        inventory.Make = makeTextBox.Text.Trim();
                        //salvam modificarile
                        ctx.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("Completati toate campurile!");
                    }
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                inventoryViewSource.View.Refresh();
                // pozitionarea pe item-ul curent
                inventoryViewSource.View.MoveCurrentTo(inventory);
            }
            else if (action == ActionState.Delete)
            {
                try
                {
                    inventory = (Inventory)inventoryDataGrid.SelectedItem;
                    ctx.Inventories.Remove(inventory);
                    ctx.SaveChanges();
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                inventoryViewSource.View.Refresh();
            }

            action = ActionState.Nothing;
            EnableElementsOnInventoryUI(action);
        }

        private void btnCancelInventory_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.Nothing;
            EnableElementsOnInventoryUI(action);
        }

        private void btnPrevInventory_Click(object sender, RoutedEventArgs e)
        {
            inventoryViewSource.View.MoveCurrentToPrevious();
        }

        private void btnNextInventory_Click(object sender, RoutedEventArgs e)
        {
            inventoryViewSource.View.MoveCurrentToNext();
        }

        void EnableElementsOnInventoryUI(ActionState state)
        {
            btnNewInventory.IsEnabled = false;
            btnEditInventory.IsEnabled = false;
            btnDeleteInventory.IsEnabled = false;
            btnPrevInventory.IsEnabled = false;
            btnNextInventory.IsEnabled = false;
            btnSaveInventory.IsEnabled = false;
            btnCancelInventory.IsEnabled = false;
            colorTextBox.IsEnabled = false;
            makeTextBox.IsEnabled = false;
            inventoryDataGrid.IsEnabled = false;

            switch (state)
            {
                case ActionState.New:
                    btnSaveInventory.IsEnabled = true;
                    btnCancelInventory.IsEnabled = true;
                    colorTextBox.IsEnabled = true;
                    makeTextBox.IsEnabled = true;
                    colorTextBox.Text = "";
                    makeTextBox.Text = "";
                    break;
                case ActionState.Edit:
                    btnSaveInventory.IsEnabled = true;
                    btnCancelInventory.IsEnabled = true;
                    colorTextBox.IsEnabled = true;
                    makeTextBox.IsEnabled = true;
                    break;
                case ActionState.Delete:
                    btnSaveInventory.IsEnabled = true;
                    btnCancelInventory.IsEnabled = true;
                    break;
                case ActionState.Nothing:
                    btnNewInventory.IsEnabled = true;
                    btnEditInventory.IsEnabled = true;
                    btnDeleteInventory.IsEnabled = true;
                    btnPrevInventory.IsEnabled = true;
                    btnNextInventory.IsEnabled = true;
                    inventoryDataGrid.IsEnabled = true;
                    inventoryDataGrid.UnselectAll();
                    colorTextBox.Text = "";
                    makeTextBox.Text = "";
                    break;
            }
        }
        #endregion
        #region Orders Menu
        private void btnNewOrders_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.New;
            EnableElementsOnOrdersUI(action);
        }

        private void btnEditOrders_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomers.SelectedItem != null && cmbInventory.SelectedItem != null)
            {
                action = ActionState.Edit;
                EnableElementsOnOrdersUI(action);
            }
            else
            {
                MessageBox.Show("Nu ati selectat o inregistrare pe care sa o editati!");
            }
        }

        private void btnDeleteOrders_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomers.SelectedItem != null && cmbInventory.SelectedItem != null)
            {
                action = ActionState.Delete;
                EnableElementsOnOrdersUI(action);
            }
            else
            {
                MessageBox.Show("Nu ati selectat o inregistrare pe care sa o stergeti!");
            }
        }

        private void btnSaveOrders_Save(object sender, RoutedEventArgs e)
        {
            Order order = null;
            if (action == ActionState.New)
            {
                try
                {
                    Customer customer = (Customer)cmbCustomers.SelectedItem;
                    Inventory inventory = (Inventory)cmbInventory.SelectedItem;
                    //instantiem Order entity
                    order = new Order()
                    {
                        CustId = customer.CustId,
                        CarId = inventory.CarId
                    };
                    //adaugam entitatea nou creata in context
                    ctx.Orders.Add(order);
                    customerOrdersViewSource.View.Refresh();
                    //salvam modificarile
                    ctx.SaveChanges();
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                BindDataGrid();
            }
            if (action == ActionState.Edit)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                try
                {
                    int curr_id = selectedOrder.OrderId;
                    var editedOrder = ctx.Orders.FirstOrDefault(s => s.OrderId == curr_id);
                    if (editedOrder != null)
                    {
                        editedOrder.CustId = Int32.Parse(cmbCustomers.SelectedValue.ToString());
                        editedOrder.CarId = Convert.ToInt32(cmbInventory.SelectedValue.ToString());
                        //salvam modificarile
                        ctx.SaveChanges();
                    }
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                BindDataGrid();
                // pozitionarea pe item-ul curent
                customerViewSource.View.MoveCurrentTo(selectedOrder);
            }
            else if (action == ActionState.Delete)
            {
                try
                {
                    dynamic selectedOrder = ordersDataGrid.SelectedItem;
                    int curr_id = selectedOrder.OrderId;
                    var deletedOrder = ctx.Orders.FirstOrDefault(s => s.OrderId == curr_id);
                    if (deletedOrder != null)
                    {
                        ctx.Orders.Remove(deletedOrder);
                        ctx.SaveChanges();
                        MessageBox.Show("Order Deleted Successfully", "Message");
                        BindDataGrid();
                    }
                }
                catch (DataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            action = ActionState.Nothing;
            EnableElementsOnOrdersUI(action);
        }

        private void btnCancelOrders_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.Nothing;
            EnableElementsOnOrdersUI(action);
        }

        private void btnPrevOrders_Click(object sender, RoutedEventArgs e)
        {
            customerOrdersViewSource.View.MoveCurrentToPrevious();
        }

        private void btnNextOrders_Click(object sender, RoutedEventArgs e)
        {
            customerOrdersViewSource.View.MoveCurrentToNext();
        }

        void EnableElementsOnOrdersUI(ActionState state)
        {
            btnNewOrders.IsEnabled = false;
            btnEditOrders.IsEnabled = false;
            btnDeleteOrders.IsEnabled = false;
            btnPrevOrders.IsEnabled = false;
            btnNextOrders.IsEnabled = false;
            btnSaveOrders.IsEnabled = false;
            btnCancelOrders.IsEnabled = false;
            cmbCustomers.IsEnabled = false;
            cmbInventory.IsEnabled = false;
            ordersDataGrid.IsEnabled = false;

            switch (state)
            {
                case ActionState.New:
                    btnSaveOrders.IsEnabled = true;
                    btnCancelOrders.IsEnabled = true;
                    cmbCustomers.IsEnabled = true;
                    cmbInventory.IsEnabled = true;
                    break;
                case ActionState.Edit:
                    btnSaveOrders.IsEnabled = true;
                    btnCancelOrders.IsEnabled = true;
                    cmbCustomers.IsEnabled = true;
                    cmbInventory.IsEnabled = true;
                    break;
                case ActionState.Delete:
                    btnSaveOrders.IsEnabled = true;
                    btnCancelOrders.IsEnabled = true;
                    break;
                case ActionState.Nothing:
                    btnNewOrders.IsEnabled = true;
                    btnEditOrders.IsEnabled = true;
                    btnDeleteOrders.IsEnabled = true;
                    btnPrevOrders.IsEnabled = true;
                    btnNextOrders.IsEnabled = true;
                    ordersDataGrid.IsEnabled = true;
                    ordersDataGrid.UnselectAll();
                    break;
            }
        }
        #endregion
    }
}
