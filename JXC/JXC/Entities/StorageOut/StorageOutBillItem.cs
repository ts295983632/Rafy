﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using System.ComponentModel;
using JXC.Commands;

namespace JXC
{
    [ChildEntity, Serializable]
    public class StorageOutBillItem : ProductRefItem
    {
        public static readonly RefProperty<StorageOutBill> StorageOutBillRefProperty =
            P<StorageOutBillItem>.RegisterRef(e => e.StorageOutBill, ReferenceType.Parent);
        public int StorageOutBillId
        {
            get { return this.GetRefId(StorageOutBillRefProperty); }
            set { this.SetRefId(StorageOutBillRefProperty, value); }
        }
        public StorageOutBill StorageOutBill
        {
            get { return this.GetRefEntity(StorageOutBillRefProperty); }
            set { this.SetRefEntity(StorageOutBillRefProperty, value); }
        }

        protected override void OnAmountChanging(ManagedPropertyChangingEventArgs<int> e)
        {
            base.OnAmountChanging(e);

            //如果出库的数据大于当前库存，应该修改错误的值为当前库存。
            if (e.Source == ManagedPropertyChangedSource.FromUIOperating && e.Value > this.Product.StorageAmount)
            {
                e.CoercedValue = this.Product.StorageAmount;
            }
        }

        protected override void OnAmountChanged(ManagedPropertyChangedEventArgs<int> e)
        {
            base.OnAmountChanged(e);

            this.RaiseRoutedEvent(PriceChangedEvent, e);
        }

        public static readonly Property<int> View_ProductStorageAmountProperty = P<StorageOutBillItem>.RegisterReadOnly(e => e.View_ProductStorageAmount, e => (e as StorageOutBillItem).GetView_ProductStorageAmount(), null);
        public int View_ProductStorageAmount
        {
            get { return this.GetProperty(View_ProductStorageAmountProperty); }
        }
        private int GetView_ProductStorageAmount()
        {
            return this.Product.StorageAmount;
        }

        #region 路由事件

        public static readonly EntityRoutedEvent PriceChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        #endregion
    }

    [Serializable]
    public class StorageOutBillItemList : ProductRefItemList { }

    public class StorageOutBillItemRepository : EntityRepository
    {
        protected StorageOutBillItemRepository() { }
    }

    internal class StorageOutBillItemConfig : EntityConfig<StorageOutBillItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("出库单项").HasDelegate(StorageOutBillItem.View_ProductNameProperty);

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(SelectProductCommand),
                typeof(BarcodeSelectProduct),
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(StorageOutBillItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageOutBillItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageOutBillItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageOutBillItem.View_ProductStorageAmountProperty).HasLabel("当前库存").ShowIn(ShowInWhere.List);
                View.Property(StorageOutBillItem.AmountProperty).HasLabel("出库数量*").ShowIn(ShowInWhere.List);
            }
        }
    }
}