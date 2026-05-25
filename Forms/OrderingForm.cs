using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantOrderingSystem.Exceptions;
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Services;
using RestaurantOrderingSystem.Utilities;
// LESSON: Lesson 12 (§2.5.2) — using ALIAS.
// Both System.Windows.Forms and RestaurantOrderingSystem.Models define
// a "MenuItem" type. We alias the model one to the short name MenuItem
// so the rest of the file can stay readable without colliding with the
// WinForms type. (Same pattern as the PDF's "using classInJoburg = ...".)
using MenuItem = RestaurantOrderingSystem.Models.MenuItem;

namespace RestaurantOrderingSystem.Forms
{
    // Cashier-facing screen. Lets the cashier pick an available meal,
    // set a quantity, add it to the order, view the running total, and
    // submit the order to produce a receipt.
    public sealed class OrderingForm : Form
    {
        private readonly ServiceContainer _ctx;
        private Order _currentOrder;

        private ComboBox cmbMeal;
        private NumericUpDown numQty;
        private Button btnAdd, btnRemove, btnSubmit, btnNew;
        private DataGridView gridOrder;
        private Label lblSubtotal, lblTax, lblTotal;

        public OrderingForm(ServiceContainer ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _currentOrder = _ctx.OrderService.CreateOrder(_ctx.CurrentUser.Username);
            BuildUi();
            RefreshOrder();
        }

        private void BuildUi()
        {
            Text = "New order";
            ClientSize = new Size(900, 560);
            BackColor = Color.FromArgb(247, 248, 252);
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(820, 520);

            var header = new Label
            {
                Text = "Build customer order",
                Font = new Font("Segoe UI Semibold", 16F),
                ForeColor = Color.FromArgb(28, 33, 48),
                Bounds = new Rectangle(20, 16, 600, 30)
            };

            // ----- picker row -----
            var lblMeal = new Label { Text = "Meal", Bounds = new Rectangle(20, 60, 100, 18), ForeColor = Color.Gray };
            cmbMeal = new ComboBox
            {
                Bounds = new Rectangle(20, 80, 420, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var item in _ctx.MenuService.GetAvailable())
            {
                cmbMeal.Items.Add(new MenuItemBox(item));
            }
            if (cmbMeal.Items.Count > 0) cmbMeal.SelectedIndex = 0;

            var lblQty = new Label { Text = "Quantity", Bounds = new Rectangle(460, 60, 100, 18), ForeColor = Color.Gray };
            numQty = new NumericUpDown
            {
                Bounds = new Rectangle(460, 80, 100, 28),
                Minimum = 1,
                Maximum = 99,
                Value = 1
            };

            btnAdd = MakeButton("Add to order", 580, 80, 140, Color.FromArgb(46, 110, 234), Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnRemove = MakeButton("Remove selected", 740, 80, 140, Color.White, Color.Firebrick);
            btnRemove.Click += BtnRemove_Click;

            // ----- grid -----
            gridOrder = new DataGridView
            {
                Bounds = new Rectangle(20, 130, 860, 280),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            gridOrder.Columns.Add("MealId", "ID");
            gridOrder.Columns.Add("MealName", "Item");
            gridOrder.Columns.Add("Qty", "Qty");
            gridOrder.Columns.Add("Price", "Unit price");
            gridOrder.Columns.Add("Total", "Line total");

            // ----- totals + actions -----
            lblSubtotal = TotalLabel("Subtotal: R 0.00", 20, 425);
            lblTax = TotalLabel("VAT (15%): R 0.00", 20, 450);
            lblTotal = TotalLabel("TOTAL: R 0.00", 20, 478);
            lblTotal.Font = new Font("Segoe UI Semibold", 13F);
            lblTotal.ForeColor = Color.FromArgb(28, 33, 48);

            btnSubmit = MakeButton("Submit order & print receipt", 540, 470, 240, Color.FromArgb(20, 161, 110), Color.White);
            btnSubmit.Click += BtnSubmit_Click;

            btnNew = MakeButton("New order", 790, 470, 90, Color.White, Color.DimGray);
            btnNew.Click += (s, e) => { _currentOrder = _ctx.OrderService.CreateOrder(_ctx.CurrentUser.Username); RefreshOrder(); };

            Controls.AddRange(new Control[] { header, lblMeal, cmbMeal, lblQty, numQty, btnAdd, btnRemove,
                                              gridOrder, lblSubtotal, lblTax, lblTotal, btnSubmit, btnNew });
        }

        private static Label TotalLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Bounds = new Rectangle(x, y, 400, 24),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 10.5F)
            };
        }

        private static Button MakeButton(string text, int x, int y, int w, Color back, Color fore)
        {
            var b = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, w, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI Semibold", 10F),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = fore == Color.White ? back : fore;
            return b;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // LESSON: Lesson 11 (§2.4.1) — specific catches before general.
            // Each branch shows a different message, so the user gets
            // contextual feedback instead of one generic "error" popup.
            try
            {
                if (cmbMeal.SelectedItem is MenuItemBox box)
                {
                    _currentOrder.AddItem(box.Item, (int)numQty.Value);
                    RefreshOrder();
                    // Re-bind the combo so updated stock counts show.
                    RefreshMealList();
                }
            }
            catch (OutOfStockException ex)
            {
                MessageBox.Show(this,
                    ex.Message + "\n\nAsk the kitchen to restock or pick another item.",
                    "Out of stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOrderStateException ex)
            {
                MessageBox.Show(this, ex.Message, "Order locked",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot add item",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshMealList()
        {
            cmbMeal.Items.Clear();
            foreach (var item in _ctx.MenuService.GetAvailable())
                cmbMeal.Items.Add(new MenuItemBox(item));
            if (cmbMeal.Items.Count > 0) cmbMeal.SelectedIndex = 0;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (gridOrder.SelectedRows.Count == 0) return;
            var id = gridOrder.SelectedRows[0].Cells["MealId"].Value?.ToString();
            if (id == null) return;
            _currentOrder.RemoveItem(id);
            RefreshOrder();
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            // LESSON: Lesson 11 (§2.4.2) — finally always runs.
            // We disable the Submit button while the work is in flight so
            // a double-click can't submit twice. Whether Submit succeeds,
            // throws InvalidOrderStateException, or throws something else
            // entirely, the finally block restores the button.
            btnSubmit.Enabled = false;
            try
            {
                _ctx.OrderService.Submit(_currentOrder);
                var receipt = ReceiptFormatter.Format(_currentOrder);

                using (var preview = new ReceiptPreviewForm(receipt))
                {
                    preview.ShowDialog(this);
                }

                _currentOrder = _ctx.OrderService.CreateOrder(_ctx.CurrentUser.Username);
                RefreshOrder();
                RefreshMealList();
            }
            catch (InvalidOrderStateException ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot submit",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot submit",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnSubmit.Enabled = true;
            }
        }

        private void RefreshOrder()
        {
            gridOrder.Rows.Clear();
            foreach (var line in _currentOrder.Items)
            {
                gridOrder.Rows.Add(
                    line.Item.MealId,
                    line.Item.MealName,
                    line.Quantity,
                    line.Item.Price.ToString("0.00"),
                    line.LineTotal.ToString("0.00"));
            }
            lblSubtotal.Text = "Subtotal: R " + _currentOrder.Subtotal.ToString("0.00");
            lblTax.Text = "VAT (15%): R " + _currentOrder.Tax.ToString("0.00");
            lblTotal.Text = "TOTAL: R " + _currentOrder.Total.ToString("0.00");
        }

        // Small helper so we can show DisplayLabel in the combo without overriding ToString on the model.
        private sealed class MenuItemBox
        {
            public MenuItem Item { get; }
            public MenuItemBox(MenuItem item) { Item = item; }
            public override string ToString()
            {
                return string.Format("{0}  —  R {1:0.00}  ({2}, stock: {3})",
                    Item.DisplayLabel, Item.Price, Item.Category, Item.StockQuantity);
            }
        }
    }
}
