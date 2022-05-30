using System.Collections.Generic;
using System.Text;
using HaT7FptBook.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace HaT7FptBook.Menu
{
    public class AdminSidebarService
    {
        private readonly IUrlHelper UrlHelper;
        public List<SidebarItem> Items { get; set; } = new List<SidebarItem>();


        public AdminSidebarService(IUrlHelperFactory factory, IActionContextAccessor action)
        {
            UrlHelper = factory.GetUrlHelper(action.ActionContext);
            // Khoi tao cac muc sidebar

            Items.Add(new SidebarItem() {Type = SidebarItemType.Divider});
            Items.Add(new SidebarItem() {Type = SidebarItemType.Heading, Title = "General Management"});
            
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "Stores",
                Action = "Index",
                Area = "StoreOwner",
                Title = "Manage Store",
                AwesomeIcon = "fas fa-folder"
            });
            
            Items.Add(new SidebarItem() {Type = SidebarItemType.Divider});

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "Orders",
                Action = "Index",
                Area = "StoreOwner",
                Title = "Manager Order",
                AwesomeIcon = "fab fa-first-order"
            });

            Items.Add(new SidebarItem() {Type = SidebarItemType.Divider});

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Manage Books",
                AwesomeIcon = "fas fa-book",
                collapseID = "role",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Books",
                        Action = "Index",
                        Area = "StoreOwner",
                        Title = "Books List"
                    },
                    new SidebarItem()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Books",
                        Action = "UpSert",
                        Area = "StoreOwner",
                        Title = "Create new Book"
                    },
                },
            });

            Items.Add(new SidebarItem() {Type = SidebarItemType.Divider});

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Manage Categories",
                AwesomeIcon = "fas fa-file",
                collapseID = "blog",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Categories",
                        Action = "Index",
                        Area = "StoreOwner",
                        Title = "Category List"
                    },
                    new SidebarItem()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Categories",
                        Action = "UpSert",
                        Area = "StoreOwner",
                        Title = "Create Category"
                    },
                },
            });

            Items.Add(new SidebarItem() {Type = SidebarItemType.Divider});
            
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "Stores",
                Action = "Help",
                Area = "StoreOwner",
                Title = "Do you need some help?",
                AwesomeIcon = "far fa-address-card"
            });
            
            Items.Add(new SidebarItem() {Type = SidebarItemType.Divider});
        }

        public string renderHtml()
        {
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(UrlHelper));
            }
            
            return html.ToString();
        }

        public void SetActive(string Controller, string Action, string Area)
        {
            foreach (var item in Items)
            {
                if (item.Controller == Controller && item.Action == Action && item.Area == Area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var childItem in item.Items)
                        {
                            if (childItem.Controller == Controller && childItem.Action == Action &&
                                childItem.Area == Area)
                            {
                                childItem.IsActive = true;
                                item.IsActive = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}