using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Services;
using RestaurantOrderingSystem.Utilities;
using MenuItem = RestaurantOrderingSystem.Models.MenuItem;

namespace RestaurantOrderingSystem.Forms
{
    // Admin-only screen. Lets the admin add, update, and delete meals on the
    // menu. All persistence goes through MenuService — the form never touches
    // the repository or the list directly.
    public sealed class MenuManagementForm : Form
    {
        private readonly ServiceContainer _ctx;

        private DataGridView    grid;
        private TextBox         txtId;
        private TextBox         txtName;
        private ComboBox        cmbCategory;
        private NumericUpDown   numPrice;
        private CheckBox        chkAvailable;
        private CheckBox        chkVegetarian;
        private NumericUpDown   numVolume;
        private Label           lblVegetarian;
        private Label           lblVolume;
        private Button          btnAdd, btnUpdate, btnDelete, btnClear;
        private TextBox         txtSearch;

        public MenuManagementForm(ServiceContainer ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            BuildUi();
            RefreshGrid();
        }

        private void BuildUi()
        {
            Text = "Menu management";
            ClientSize = new Size(960, 560);
            BackColor = Color.FromArgb(247, 248, 252);
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(860, 520);

            var header = new Label
            {
                Text = "Menu management",
                Font = new Font("Segoe UI Semibold", 16F),
                ForeColor = Color.FromArgb(28, 33, 48),
                Bounds = new Rectangle(20, 16, 600, 30)
            };

            txtSearch = new TextBox { Bounds = new Rectangle(620, 22, 240, 26), Font = new Font("Segoe UI", 10F) };
            txtSearch.TextChanged += (s, e) => RefreshGrid();
            var lblSearch = new Label { Text = "🔎", Bounds = new Rectangle(870, 22, 30, 26) };

            grid = new DataGridView
            {
                Bounds = new Rectangle(20, 60, 920, 280),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grid.Columns.Add("MealId",    "ID");
            grid.Columns.Add("MealName",  "Name");
            grid.Columns.Add("Type",      "Type");
            grid.Columns.Add("Category",  "Category");
            grid.Columns.Add("Price",     "Price");
            grid.Columns.Add("Available", "Available");
            grid.Columns.Add("Extra",     "Extra");
            grid.SelectionChanged += Grid_SelectionChanged;

            // ---- Form panel ----
            var formPanel = new Panel
            {
                Bounds = new Rectangle(20, 360, 920, 180),
                BackColor = Color.White,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            int col1 = 20, col2 = 240, col3 = 460, col4 = 680;
            int rowY1 = 20, rowY2 = 80;

            formPanel.Controls.Add(new Label { Text = "Meal ID",  Bounds = new Rectangle(col1, rowY1, 200, 18), ForeColor = Color.Gray });
            txtId = new TextBox { Bounds = new Rectangle(col1, rowY1 + 20, 200, 28) };
            formPanel.Controls.Add(txtId);

            formPanel.Controls.Add(new Label { Text = "Name",     Bounds = new Rectangle(col2, rowY1, 200, 18), ForeColor = Color.Gray });
            txtName = new TextBox { Bounds = new Rectangle(col2, rowY1 + 20, 200, 28) };
            formPanel.Controls.Add(txtName);

            formPanel.Controls.Add(new Label { Text = "Category", Bounds = new Rectangle(col3, rowY1, 200, 18), ForeColor = Color.Gray });
            cmbCategory = new ComboBox
            {
                Bounds = new Rectangle(col3, rowY1 + 20, 200, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (MealCategory c in Enum.GetValues(typeof(MealCategory))) cmbCategory.Items.Add(c);
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => UpdateExtraFields();
            formPanel.Controls.Add(cmbCategory);

            formPanel.Controls.Add(new Label { Text = "Price (R)", Bounds = new Rectangle(col4, rowY1, 200, 18), ForeColor = Color.Gray });
            numPrice = new NumericUpDown
            {
                Bounds = new Rectangle(col4, rowY1 + 20, 200, 28),
                DecimalPlaces = 2, Increment = 1m, Maximum = 100000m, Minimum = 0m
            };
            formPanel.Controls.Add(numPrice);

            chkAvailable = new CheckBox
            {
                Text = "Available",
                Checked = true,
                Bounds = new Rectangle(col1, rowY2, 140, 28)
            };
            formPanel.Controls.Add(chkAvailable);

            chkVegetarian = new CheckBox
            {
                Text = "Vegetarian (food only)",
                Bounds = new Rectangle(col2, rowY2, 200, 28)
            };
            lblVegetarian = new Label { Visible = false }; // placeholder, kept for symmetry
            formPanel.Controls.Add(chkVegetarian);

            lblVolume = new Label { Text = "Volume (ml, drinks only)", Bounds = new Rectangle(col3, rowY2 - 18, 200, 18), ForeColor = Color.Gray };
            numVolume = new NumericUpDown
            {
                Bounds = new Rectangle(col3, rowY2, 200, 28),
                Maximum = 10000, Minimum = 0, Increment = 50
            };
            formPanel.Controls.Add(lblVolume);
            formPanel.Controls.Add(numVolume);

            btnAdd    = MakeButton("Add",    col4,        rowY2,   90, Color.FromArgb(46, 110, 234), Color.White);
            btnUpdate = MakeButton("Update", col4 + 100,  rowY2,   90, Color.White, Color.FromArgb(46, 110, 234));
            btnDelete = MakeButton("Delete", col1,        rowY2 + 36, 90, Color.White, Color.Firebrick);
            btnClear  = MakeButton("Clear",  col1 + 100,  rowY2 + 36, 90, Color.White, Color.DimGray);

            btnAdd.Click    += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click  += (s, e) => ClearForm();

            formPanel.Controls.Add(btnAdd);
            formPanel.Controls.Add(btnUpdate);
            formPanel.Controls.Add(btnDelete);
            formPanel.Controls.Add(btnClear);

            Controls.AddRange(new Control[] { header, lblSearch, txtSearch, grid, formPanel });
            UpdateExtraFields();
        }

        private static Button MakeButton(string text, int x, int y, int w, Color back, Color fore)
        {
            var b = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, w, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI Semibold", 9.5F),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = fore == Color.White ? back : fore;
            return b;
        }

        private void UpdateExtraFields()
        {
            var isDrink = (MealCategory)cmbCategory.SelectedItem == MealCategory.Drink;
            chkVegetarian.Visible = !isDrink;
            numVolume.Visible     = isDrink;
            lblVolume.Visible     = isDrink;
        }

        private void RefreshGrid()
        {
            var items = string.IsNullOrWhiteSpace(txtSearch?.Text)
                ? _ctx.MenuService.GetAll()
                : _ctx.Menu.Search(txtSearch.Text);

            grid.Rows.Clear();
            foreach (var item in items)
            {
                string type  = item is FoodItem ? "Food" : "Drink";
                string extra = item is FoodItem food
                    ? (food.IsVegetarian ? "Vegetarian" : "—")
                    : item is DrinkItem drink ? drink.VolumeMl + " ml" : "—";

                grid.Rows.Add(item.MealId, item.MealName, type, item.Category, item.Price.ToString("0.00"),
                              item.IsAvailable ? "Yes" : "No", extra);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var id = grid.SelectedRows[0].Cells["MealId"].Value?.ToString();
            var item = _ctx.MenuService.GetAll().FirstOrDefault(m => m.MealId == id);
            if (item == null) return;

            txtId.Text = item.MealId;
            txtId.ReadOnly = true; // ID cannot change after creation
            txtName.Text = item.MealName;
            cmbCategory.SelectedItem = item.Category;
            numPrice.Value = item.Price;
            chkAvailable.Checked = item.IsAvailable;

            if (item is FoodItem food)
            {
                chkVegetarian.Checked = food.IsVegetarian;
                numVolume.Value = 0;
            }
            else if (item is DrinkItem drink)
            {
                chkVegetarian.Checked = false;
                numVolume.Value = drink.VolumeMl;
            }
            UpdateExtraFields();
        }

        private void ClearForm()
        {
            txtId.ReadOnly = false;
            txtId.Text = string.Empty;
            txtName.Text = string.Empty;
            cmbCategory.SelectedIndex = 0;
            numPrice.Value = 0;
            chkAvailable.Checked = true;
            chkVegetarian.Checked = false;
            numVolume.Value = 0;
            grid.ClearSelection();
            UpdateExtraFields();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Validator.RequireText(txtId.Text, "Meal ID");
                Validator.RequireText(txtName.Text, "Name");

                var cat = (MealCategory)cmbCategory.SelectedItem;
                if (cat == MealCategory.Drink)
                    _ctx.MenuService.AddDrink(txtId.Text, txtName.Text, numPrice.Value, chkAvailable.Checked, (int)numVolume.Value);
                else
                    _ctx.MenuService.AddFood(txtId.Text, txtName.Text, cat, numPrice.Value, chkAvailable.Checked, chkVegetarian.Checked);

                RefreshGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot add", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                Validator.RequireText(txtId.Text, "Meal ID");
                Validator.RequireText(txtName.Text, "Name");

                var cat = (MealCategory)cmbCategory.SelectedItem;
                MenuItem replacement = cat == MealCategory.Drink
                    ? (MenuItem)new DrinkItem(txtId.Text, txtName.Text, numPrice.Value, chkAvailable.Checked, (int)numVolume.Value)
                    : new FoodItem(txtId.Text, txtName.Text, cat, numPrice.Value, chkAvailable.Checked, chkVegetarian.Checked);

                _ctx.MenuService.Update(replacement);
                RefreshGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text)) return;
            var confirm = MessageBox.Show(this, "Delete this meal?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            try
            {
                _ctx.MenuService.Delete(txtId.Text);
                RefreshGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
