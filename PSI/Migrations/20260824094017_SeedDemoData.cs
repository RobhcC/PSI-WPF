using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PSI.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "ContactPerson", "Name", "Phone" },
                values: new object[,]
                {
                    { 4, "广州市番禺区XX路18号", "黄工", "广州明辉食品机械", "13044441111" },
                    { 5, "青岛市黄岛区XX路77号", "郑工", "青岛海纳重工装备", "12855552222" },
                    { 6, "成都市高新区XX路32号", "何工", "成都锦程自动化集成", "12766663333" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "Name", "PurchasePrice", "SalePrice", "Unit" },
                values: new object[,]
                {
                    { 9, "驱动", "伺服电机 750W", 1350m, 1750m, "台" },
                    { 10, "线材", "编码器线缆 2米", 45m, 75m, "根" },
                    { 11, "低压电器", "空气开关 2P 32A", 28m, 48m, "个" },
                    { 12, "传感器", "光电开关漫反射", 55m, 88m, "个" },
                    { 13, "软件", "组态软件授权", 800m, 1200m, "套" },
                    { 14, "通讯", "工业交换机 8口", 160m, 260m, "台" },
                    { 15, "低压电器", "信号隔离器", 65m, 105m, "个" },
                    { 16, "低压电器", "急停按钮盒", 22m, 40m, "个" }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrders",
                columns: new[] { "Id", "CreatedAt", "OrderDate", "OrderNo", "SupplierId", "TotalAmount" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 8, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 8, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260408093000", 3, 36360m },
                    { 2, new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260512090000", 1, 16650m },
                    { 5, new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260810093000", 2, 34740m }
                });

            migrationBuilder.InsertData(
                table: "SaleOrders",
                columns: new[] { "Id", "CreatedAt", "CustomerId", "OrderDate", "OrderNo", "TotalAmount" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 20, 9, 30, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 5, 20, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260520093000", 14850m },
                    { 2, new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260625090000", 15280m }
                });

            migrationBuilder.InsertData(
                table: "StockLogs",
                columns: new[] { "Id", "ChangeType", "CreatedAt", "OrderNo", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, "采购入库", new DateTime(2026, 4, 8, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260408093000", 1, 10 },
                    { 2, "采购入库", new DateTime(2026, 4, 8, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260408093000", 2, 6 },
                    { 3, "采购入库", new DateTime(2026, 4, 8, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260408093000", 3, 12 },
                    { 4, "采购入库", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260512090000", 4, 200 },
                    { 5, "采购入库", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260512090000", 6, 300 },
                    { 6, "采购入库", new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260512090000", 8, 50 },
                    { 7, "采购入库", new DateTime(2026, 6, 15, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260615093000", 5, 150 },
                    { 10, "采购入库", new DateTime(2026, 7, 18, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260718090000", 1, 5 },
                    { 13, "采购入库", new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260810093000", 7, 1000 },
                    { 18, "销售出库", new DateTime(2026, 5, 20, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260520093000", 1, 4 },
                    { 19, "销售出库", new DateTime(2026, 5, 20, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260520093000", 3, 5 },
                    { 20, "销售出库", new DateTime(2026, 5, 20, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260520093000", 6, 100 },
                    { 21, "销售出库", new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260625090000", 2, 3 },
                    { 22, "销售出库", new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260625090000", 4, 80 },
                    { 23, "销售出库", new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260625090000", 8, 20 },
                    { 24, "销售出库", new DateTime(2026, 7, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260728090000", 5, 60 }
                });

            migrationBuilder.InsertData(
                table: "Stocks",
                columns: new[] { "Id", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 11 },
                    { 2, 2, 3 },
                    { 3, 3, 7 },
                    { 4, 4, 120 },
                    { 5, 5, 90 },
                    { 6, 6, 200 },
                    { 7, 7, 1000 },
                    { 8, 8, 30 }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "Address", "ContactPerson", "Name", "Phone" },
                values: new object[,]
                {
                    { 4, "北京市海淀区XX路15号", "孙工", "北京中科自控设备", "13311117777" },
                    { 5, "武汉市东湖高新区XX路9号", "周工", "武汉光谷传感器科技", "13222228888" },
                    { 6, "东莞市长安镇XX路56号", "吴老板", "东莞长安机电市场", "13133339999" }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrderDetails",
                columns: new[] { "Id", "Amount", "ProductId", "PurchaseOrderId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 15000m, 1, 1, 10, 1500m },
                    { 2, 13200m, 2, 1, 6, 2200m },
                    { 3, 8160m, 3, 1, 12, 680m },
                    { 4, 7000m, 4, 2, 200, 35m },
                    { 5, 5400m, 6, 2, 300, 18m },
                    { 6, 4250m, 8, 2, 50, 85m },
                    { 13, 3500m, 7, 5, 1000, 3.5m },
                    { 14, 9000m, 10, 5, 200, 45m },
                    { 15, 2240m, 11, 5, 80, 28m },
                    { 16, 16000m, 13, 5, 20, 800m },
                    { 17, 4000m, 14, 5, 25, 160m }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrders",
                columns: new[] { "Id", "CreatedAt", "OrderDate", "OrderNo", "SupplierId", "TotalAmount" },
                values: new object[,]
                {
                    { 3, new DateTime(2026, 6, 15, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 15, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260615093000", 5, 17660m },
                    { 4, new DateTime(2026, 7, 18, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 18, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260718090000", 4, 22200m }
                });

            migrationBuilder.InsertData(
                table: "SaleOrderDetails",
                columns: new[] { "Id", "Amount", "ProductId", "Quantity", "SaleOrderId", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 7400m, 1, 4, 1, 1850m },
                    { 2, 4450m, 3, 5, 1, 890m },
                    { 3, 3000m, 6, 100, 1, 30m },
                    { 4, 8040m, 2, 3, 2, 2680m },
                    { 5, 4640m, 4, 80, 2, 58m },
                    { 6, 2600m, 8, 20, 2, 130m }
                });

            migrationBuilder.InsertData(
                table: "SaleOrders",
                columns: new[] { "Id", "CreatedAt", "CustomerId", "OrderDate", "OrderNo", "TotalAmount" },
                values: new object[,]
                {
                    { 3, new DateTime(2026, 7, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), 4, new DateTime(2026, 7, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260728090000", 11700m },
                    { 4, new DateTime(2026, 8, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), 5, new DateTime(2026, 8, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260815090000", 15850m },
                    { 5, new DateTime(2026, 8, 22, 9, 30, 0, 0, DateTimeKind.Unspecified), 6, new DateTime(2026, 8, 22, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260822093000", 14190m }
                });

            migrationBuilder.InsertData(
                table: "StockLogs",
                columns: new[] { "Id", "ChangeType", "CreatedAt", "OrderNo", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 8, "采购入库", new DateTime(2026, 6, 15, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260615093000", 12, 120 },
                    { 9, "采购入库", new DateTime(2026, 6, 15, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260615093000", 16, 80 },
                    { 11, "采购入库", new DateTime(2026, 7, 18, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260718090000", 9, 8 },
                    { 12, "采购入库", new DateTime(2026, 7, 18, 9, 0, 0, 0, DateTimeKind.Unspecified), "CG20260718090000", 15, 60 },
                    { 14, "采购入库", new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260810093000", 10, 200 },
                    { 15, "采购入库", new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260810093000", 11, 80 },
                    { 16, "采购入库", new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260810093000", 13, 20 },
                    { 17, "采购入库", new DateTime(2026, 8, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), "CG20260810093000", 14, 25 },
                    { 25, "销售出库", new DateTime(2026, 7, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260728090000", 12, 50 },
                    { 26, "销售出库", new DateTime(2026, 7, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260728090000", 16, 40 },
                    { 27, "销售出库", new DateTime(2026, 8, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260815090000", 9, 5 },
                    { 28, "销售出库", new DateTime(2026, 8, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260815090000", 14, 10 },
                    { 29, "销售出库", new DateTime(2026, 8, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "XS20260815090000", 10, 60 },
                    { 30, "销售出库", new DateTime(2026, 8, 22, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260822093000", 13, 8 },
                    { 31, "销售出库", new DateTime(2026, 8, 22, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260822093000", 11, 30 },
                    { 32, "销售出库", new DateTime(2026, 8, 22, 9, 30, 0, 0, DateTimeKind.Unspecified), "XS20260822093000", 15, 30 }
                });

            migrationBuilder.InsertData(
                table: "Stocks",
                columns: new[] { "Id", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 9, 9, 3 },
                    { 10, 10, 140 },
                    { 11, 11, 50 },
                    { 12, 12, 70 },
                    { 13, 13, 12 },
                    { 14, 14, 15 },
                    { 15, 15, 30 },
                    { 16, 16, 40 }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrderDetails",
                columns: new[] { "Id", "Amount", "ProductId", "PurchaseOrderId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 7, 9300m, 5, 3, 150, 62m },
                    { 8, 6600m, 12, 3, 120, 55m },
                    { 9, 1760m, 16, 3, 80, 22m },
                    { 10, 7500m, 1, 4, 5, 1500m },
                    { 11, 10800m, 9, 4, 8, 1350m },
                    { 12, 3900m, 15, 4, 60, 65m }
                });

            migrationBuilder.InsertData(
                table: "SaleOrderDetails",
                columns: new[] { "Id", "Amount", "ProductId", "Quantity", "SaleOrderId", "UnitPrice" },
                values: new object[,]
                {
                    { 7, 5700m, 5, 60, 3, 95m },
                    { 8, 4400m, 12, 50, 3, 88m },
                    { 9, 1600m, 16, 40, 3, 40m },
                    { 10, 8750m, 9, 5, 4, 1750m },
                    { 11, 2600m, 14, 10, 4, 260m },
                    { 12, 4500m, 10, 60, 4, 75m },
                    { 13, 9600m, 13, 8, 5, 1200m },
                    { 14, 1440m, 11, 30, 5, 48m },
                    { 15, 3150m, 15, 30, 5, 105m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderDetails",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "SaleOrderDetails",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "StockLogs",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SaleOrders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SaleOrders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SaleOrders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SaleOrders",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SaleOrders",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
