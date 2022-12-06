using Microsoft.Win32;
using RegistryValley.App.Models;
using System.Windows;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public class DataContainer : ObservableObject
    {
        public DataContainer()
        {
            _hiveSource = new();

            _valueSource = new();

            InitializeRegistryRoot();
        }

        private ObservableCollection<HiveItem> _hiveSource;
        public ObservableCollection<HiveItem> HiveSource { get => _hiveSource; set => SetProperty(ref _hiveSource, value); }

        private ObservableCollection<HiveItem> _valueSource;
        public ObservableCollection<HiveItem> ValueSource { get => _valueSource; set => SetProperty(ref _valueSource, value); }

        private readonly string folderImageSource = "ms-appx:///Assets/Images/Folder.png";
        private readonly string computerImageSource = "ms-appx:///Assets/Images/Computer.png";
        private readonly string binaryImageSource = "ms-appx:///Assets/Images/Binary.png";
        private readonly string stringImageSource = "ms-appx:///Assets/Images/String.png";

        private void InitializeRegistryRoot()
        {
            List<HiveItem> list = new()
            {
                new()
                {
                    Name = "HKEY_CLASSES_ROOT",
                    Image = folderImageSource,
                    Hive = RegistryHive.ClassesRoot,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_CURRENT_USER",
                    Image = folderImageSource,
                    Hive = RegistryHive.CurrentUser,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_LOCAL_MACHINE",
                    Image = folderImageSource,
                    Hive = RegistryHive.LocalMachine,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_USERS",
                    Image = folderImageSource,
                    Hive = RegistryHive.Users,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_CURRENT_CONFIG",
                    Image = folderImageSource,
                    Hive = RegistryHive.CurrentConfig,
                    Path = ""
                }
            };

            foreach (HiveItem element in list)
            {
                try
                {
                    string[] keys = Array.Empty<string>();

                    switch (element.Hive)
                    {
                        case RegistryHive.ClassesRoot:
                            keys = Registry.ClassesRoot.GetSubKeyNames();
                            break;
                        case RegistryHive.CurrentConfig:
                            keys = Registry.CurrentConfig.GetSubKeyNames();
                            break;
                        case RegistryHive.CurrentUser:
                            keys = Registry.CurrentUser.GetSubKeyNames();
                            break;
                        case RegistryHive.LocalMachine:
                            keys = Registry.LocalMachine.GetSubKeyNames();
                            break;
                        case RegistryHive.Users:
                            keys = Registry.Users.GetSubKeyNames();
                            break;
                    }

                    foreach (string key in keys)
                    {
                        element.Children.Add(new()
                        {
                            Name = key,
                            Hive = element.Hive,
                            Image = folderImageSource,
                            Path = element.Path + key + "\\"
                        });
                    }
                }
                catch
                {

                }
            }

            _hiveSource.Add(new()
            {
                Name = "Computer",
                Children = new(list),
                Image = computerImageSource,
                Expanded = true
            });
        }

        public void ItemExpanded(HiveItem selectedItem)
        {
            //if (lastInvokedItem != null)
            //{
            //    if (GetPath(selectedItem).ToLower().Equals(GetPath(lastInvokedItem).ToLower(), StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        return;
            //    }
            //}

            //if (selectedItem.Expanded == true)
            //    return;

            //lastInvokedItem = selectedItem;
            //selectedItem.Expanded = true;

            //foreach (HiveItem element in selectedItem.Children)
            //{
            //    if (element.Children.Count > 0)
            //        break;

            //    App.registry.GetSubKeyList(element.Hive, element.Path, out string[] keys);

            //    if (keys == null)
            //        continue;

            //    foreach (string key in keys)
            //    {
            //        HiveItem item = new()
            //        {
            //            Name = key,
            //            Hive = element.Hive,
            //            Image = folderImageSource,
            //            Path = element.Path + key + "\\"
            //        };

            //        element.Children.Add(item);
            //    }
            //}
        }

        public void ItemCollapsed(HiveItem selectedItem)
        {
            selectedItem.Expanded = false;
        }

        private HiveItem lastInvokedItem;

        private void OnPathChanged(string value)
        {
            if (lastInvokedItem != null)
            {
                if (value.ToLower().Equals(GetPath(lastInvokedItem).ToLower(), StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }
            }

            var elements = value.Split('\\');
            int elementIndex = 0;
            var treeitems = HiveSource;

            while (elementIndex < elements.Length)
            {
                bool foundItem = false;
                foreach (var treeitem in treeitems)
                {
                    if (treeitem.Name.ToLower().Equals(elements[elementIndex].ToLower(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        elementIndex++;
                        treeitems = treeitem.Children;
                        foundItem = true;
                        ItemExpanded(treeitem);
                        if (elementIndex == elements.Length)
                            ItemInvoked(treeitem);
                        break;
                    }
                }

                if (!foundItem)
                    break;
            }
        }

        private string GetPath(HiveItem item)
        {
            if (item.Name == "Computer" && string.IsNullOrEmpty(item.Path))
            {
                return item.Name;
            }
            else if (string.IsNullOrEmpty(item.Path))
            {
                return "Computer\\" + item.Hive.ToString();
            }
            else
            {
                return "Computer\\" + item.Hive.ToString() + "\\" + item.Path.TrimEnd('\\');
            }
        }

        public void ItemInvoked(HiveItem item)
        {
            lastInvokedItem = item;

            ValueSource.Clear();

            //Path = GetPath(item);

            //if (!(item.Name == "Computer" && string.IsNullOrEmpty(item.Path)))
            //{
            //    RegistryType dtype = RegistryType.String;
            //    byte[] dbuf = System.Text.Encoding.Unicode.GetBytes("(value not set)");

            //    try
            //    {
            //        App.registry.QueryValue(item.Hive, item.Path, null, out dtype, out dbuf);
            //    }
            //    catch { };

            //    string ddatastr = "(value not set)";

            //    if (dtype == RegistryType.String)
            //    {
            //        ddatastr = System.Text.Encoding.Unicode.GetString(dbuf);
            //    }
            //    else if (dtype == RegistryType.Integer)
            //    {
            //        ddatastr = BitConverter.ToInt32(dbuf, 0).ToString();
            //    }
            //    else if (dtype == RegistryType.Long)
            //    {
            //        ddatastr = BitConverter.ToInt64(dbuf, 0).ToString();
            //    }
            //    else
            //    {
            //        ddatastr = BitConverter.ToString(dbuf);
            //    }

            //    ValueSource.Add(new ValueItem()
            //    {
            //        Name = "(Default)",
            //        Type = dtype.ToString(),
            //        Data = ddatastr,
            //        Image = textImageSource,
            //        ParentItem = item
            //    });

            //    App.registry.GetValueList(item.Hive, item.Path, out string[] list);

            //    if (list != null && list.Length != 0)
            //    {
            //        var valuerange = (list.ToList().OrderBy(x => x).Select(x =>
            //        {
            //            RegistryType vtype = App.registry.GetValueInfo(item.Hive, item.Path, x, 0);

            //            ValueItem vitem = new()
            //            {
            //                Name = x,
            //                Type = vtype.ToString(),
            //                ParentItem = item
            //            };

            //            App.registry.QueryValue(item.Hive, item.Path, x, out vtype, out byte[] buf);

            //            if (vtype == RegistryType.String)
            //            {
            //                vitem.Data = System.Text.Encoding.Unicode.GetString(buf);
            //                vitem.Image = textImageSource;
            //            }
            //            else if (vtype == RegistryType.Integer)
            //            {
            //                vitem.Data = BitConverter.ToInt32(buf, 0).ToString();
            //                vitem.Image = numbersImageSource;
            //            }
            //            else if (vtype == RegistryType.Long)
            //            {
            //                vitem.Data = BitConverter.ToInt64(buf, 0).ToString();
            //                vitem.Image = numbersImageSource;
            //            }
            //            else
            //            {
            //                vitem.Data = BitConverter.ToString(buf);
            //                vitem.Image = numbersImageSource;
            //            }

            //            return vitem;
            //        }));

            //        foreach (var value in valuerange)
            //            ValueSource.Add(value);
            //    }
            //}
        }
    }
}
