using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AirVentsCadWpf.DataControls;
using AirVentsCadWpf.DataControls.Specification;
using AirVentsCadWpf.DataControls.ForConstructors;
using FramelessUnitUc = AirVentsCadWpf.DataControls.FrameLessUnit.FramelessUnitUc;

namespace AirVentsCadWpf.MenuControls
{
    /// <summary>
    /// Interaction logic for MenuTree.xaml
    /// </summary>
    public partial class MenuTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuTree"/> class. Дерево елементов.
        /// </summary>
        public MenuTree()
        {
            InitializeComponent();
        }

        private void MenuTree1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = MenuTree1.SelectedItem as TreeViewItem;
            if (item == null){return;}
            switch (item.Header.ToString()) 
            {
                case "Заказы":
                    Switcher.SwitchData(new OrdersUc());
                    break;
                case "Спецификация":
                    Switcher.SwitchData(new SpecificationUc()); 
                    break;
                case "Спецификации":
                    Switcher.SwitchData(new SpecificationBomUc());
                    break;
                case "Перечень деталей":
                    Switcher.SwitchData(new Bom());
                    break;                
                case "Бескаркасная":
                    Switcher.SwitchData(new FramelessUnitUc
                    {
                        Lenght = { Text = "650" }
                    });
                    break;
                case "Конструкторам":
                    Switcher.SwitchData(new ForConstructorsUc());
                    break;
                case "Моноблок":
                    Switcher.SwitchData(new MonoBlock01Uc50
                    {
                        RoofOfUnit50 = { IsChecked = true },
                        InnerOfUnit50 = { IsChecked = true },
                        Panel50 = { IsChecked = true },
                        MontageFrame50 = { IsChecked = true },
                        Lenght = { Text = "1000" }
                    });
                    break;
                case "Установки":
                   // Switcher.SwitchData(new Units50Uc());
                    break;
                case "Блок корпуса":
                     Switcher.SwitchData(new Unit50Uc
                     {
                         RoofOfUnit50 = { IsChecked = true },
                         InnerOfUnit50 = { IsChecked = true },
                         Panel50 = { IsChecked = true },
                         MontageFrame50 = { IsChecked = true },
                         Lenght = { Text = "1000" }
                     });
                    break;
                case "Панель":
                    Switcher.SwitchData(new Panel50Uc());
                    break;
                case "Рама монтажная":
                    Switcher.SwitchData(new MontageFrameUc());
                    break;
                case "Вибровставка":
                    Switcher.SwitchData(new SpigotUc());
                    break;
                case "Заслонка":
                    Switcher.SwitchData(new DamperUc());
                    break;
                case "Крыша":
                    Switcher.SwitchData(new RoofUc());
                    break;
                case "Патрубок противодождевой":
                    Switcher.SwitchData(new RoofUc());
                    break;
                case  "5": break;
            }
        }

        static class TreeViewHelper
        {
            public static void ExpandAll(TreeView treeView)
            {
                ExpandSubContainers(treeView);
            }

            private static void ExpandSubContainers(ItemsControl parentContainer)
            {
                foreach (var item in parentContainer.Items)
                {
                    var currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    if (currentContainer != null && currentContainer.Items.Count > 0)
                    {
                        // Expand the current item. 
                        currentContainer.IsExpanded = true;
                        if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                        {
                            // If the sub containers of current item is not ready, we need to wait until 
                            // they are generated. 
                            currentContainer.ItemContainerGenerator.StatusChanged += delegate
                            {
                                ExpandSubContainers(currentContainer);
                            };
                        }
                        else
                        {
                            // If the sub containers of current item is ready, we can directly go to the next 
                            // iteration to expand them. 
                            ExpandSubContainers(currentContainer);
                        }
                    }
                }
            }
        }

        private void MenuTree1_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewHelper.ExpandAll(MenuTree1);
        }

    }
}
