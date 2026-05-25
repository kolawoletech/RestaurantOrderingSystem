using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantOrderingSystem.Exceptions;
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Services;

namespace RestaurantOrderingSystem.Forms
{
    // LESSON: Lesson 11 — try/catch in the UI layer.
    //
    // This form does NOT mutate Order.Status directly. It calls OrderService,
    // which calls the controlled transition methods on Order. If a transition
    // is illegal (e.g. trying to "Mark Ready" something that is already
    // Completed), the model THROWS InvalidOperationException and the catch
    // block below shows the message in a MessageBox. This is the textbook
    // L11 division of labour: low-level code throws specific exceptions,
    // high-level (UI) code catches and presents them to the user.
    public sealed class KitchenForm : Form
    {
        private readonly ServiceContainer _ctx;

        private ComboBox cmbFilter;
        private DataGridView gridOrders;
        private Button btnReady, btnComplete, btnCancel, btnRefresh;
        private Label lblHint;

        public KitchenForm(ServiceContainer ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            BuildUi();
            Refresh();
        }

        private void BuildUi()
        {
            Text = "Kitchen — Order Status";
            ClientSize = new Size(900, 560);
            BackColor = Color.FromArgb(247, 248, 252);
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(820, 520);

            var header = new Label
            {
                Text = "Lively kitchen queue",
                Font = new Font("Times New Roman", 16F),
                ForeColor = Color.FromArgb(28, 33, 48),
                Bounds = new Rectangle(20, 16, 600, 30)
            };

            var lblFilter = new Label { Text = "Show status", Bounds = new Rectangle(20, 60, 100, 18), ForeColor = Color.Gray };
            cmbFilter = new ComboBox
            {
                Bounds = new Rectangle(20, 80, 200, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilter.Items.Add("All");
            foreach (OrderStatus s in Enum.GetValues(typeof(OrderStatus)))
                cmbFilter.Items.Add(s);
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => Refresh();

            btnRefresh = MakeButton("Refresh", 240, 80, 100, Color.White, Color.DimGray);
            btnRefresh.Click += (s, e) => Refresh();

            gridOrders = new DataGridView
            {
                Bounds = new Rectangle(20, 130, 860, 320),
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
            gridOrders.Columns.Add("OrderNumber", "Order #");
            gridOrders.Columns.Add("Cashier", "Cashier");
            gridOrders.Columns.Add("Items", "Items");
            gridOrders.Columns.Add("Total", "Total");
            gridOrders.Columns.Add("Status", "Status");
            gridOrders.Columns.Add("Created", "Created");

            lblHint = new Label
            {
                Text = "Select an order, then choose an action. Pending → Preparing happens automatically when the cashier submits.",
                Bounds = new Rectangle(20, 460, 860, 20),
                ForeColor = Color.Gray
            };

            btnReady = MakeButton("Mark Ready", 20, 490, 150, Color.FromArgb(46, 110, 234), Color.White);
            btnComplete = MakeButton("Mark Completed", 180, 490, 170, Color.FromArgb(20, 161, 110), Color.White);
            btnCancel = MakeButton("Cancel Order", 360, 490, 140, Color.White, Color.Firebrick);

            btnReady.Click += (s, e) => Apply((svc, num) => svc.MarkReady(num), "mark ready");
            btnComplete.Click += (s, e) => Apply((svc, num) => svc.MarkCompleted(num), "complete");
            btnCancel.Click += (s, e) => Apply((svc, num) => svc.Cancel(num), "cancel");

            Controls.AddRange(new Control[] {
                header, lblFilter, cmbFilter, btnRefresh,
                gridOrders, lblHint, btnReady, btnComplete, btnCancel
            });
        }

        private void Apply(Action<OrderService, int> action, string label)
        {
            if (gridOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Select an order first.", "No selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var cell = gridOrders.SelectedRows[0].Cells["OrderNumber"].Value;
            if (cell == null) return;
            int orderNumber = Convert.ToInt32(cell);

            // LESSON: Lesson 11 (§2.4.1) — multiple catch clauses,
            // ordered most-specific to least-specific. The custom type
            // tells us *what kind* of problem occurred without parsing
            // the message string.
            try
            {
                action(_ctx.OrderService, orderNumber);
                Refresh();
            }
            catch (InvalidOrderStateException ex)
            {
                MessageBox.Show(this,
                    ex.Message + "\n\nTip: orders flow Pending → Preparing → Ready → Completed.",
                    "Cannot " + label, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot " + label,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            if (gridOrders == null) return;

            gridOrders.Rows.Clear();

            var filter = cmbFilter.SelectedItem;
            var orders = (filter is OrderStatus s)
                ? _ctx.OrderService.GetByStatus(s)
                : _ctx.OrderService.GetAll();

            int totalItems;
            foreach (var o in orders)
            {
                totalItems = 0;
                foreach (var line in o.Items) totalItems += line.Quantity;

                int row = gridOrders.Rows.Add(
                    o.OrderNumber,
                    o.CashierUsername,
                    totalItems,
                    "R " + o.Total.ToString("0.00"),
                    o.Status,
                    o.CreatedAt.ToString("HH:mm"));

                gridOrders.Rows[row].DefaultCellStyle.BackColor = ColorFor(o.Status);
            }
        }

        private static Color ColorFor(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending: return Color.FromArgb(255, 248, 220);
                case OrderStatus.Preparing: return Color.FromArgb(224, 238, 255);
                case OrderStatus.Ready: return Color.FromArgb(220, 245, 230);
                case OrderStatus.Completed: return Color.White;
                case OrderStatus.Cancelled: return Color.FromArgb(250, 224, 224);
                default: return Color.White;
            }
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
    }
}
