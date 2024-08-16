import { Line, Pie } from "@ant-design/charts";
import { Table, Tabs } from "antd";
import TabPane from "antd/es/tabs/TabPane";
import React from "react";

const Profile = () => {
  const config = {
    data: [
      { type: "ETH", value: 27 },
      { type: "BTC", value: 25 },
      { type: "LTC", value: 18 },
      { type: "XRP", value: 15 },
      { type: "DOGE", value: 10 },
      { type: "OMG", value: 5 },
    ],
    angleField: "value",
    colorField: "type",
    paddingRight: 80,
    innerRadius: 0.6,
    label: {
      text: "type",
      style: {
        fontWeight: "bold",
      },
    },
    legend: {
      color: {
        title: false,
        position: "right",
        rowPadding: 5,
      },
    },
    annotations: [
      {
        type: "text",
        style: {
          text: "Breakdown",
          x: "50%",
          y: "50%",
          textAlign: "center",
          fontSize: 40,
          fontStyle: "bold",
        },
      },
    ],
  };
  const lineData = [
    { year: "1991", value: 3 },
    { year: "1992", value: 4 },
    { year: "1993", value: 3.5 },
    { year: "1994", value: 5 },
    { year: "1995", value: 4.9 },
    { year: "1996", value: 6 },
    { year: "1997", value: 7 },
    { year: "1998", value: 9 },
    { year: "1999", value: 13 },
  ];
  const lineConfig = {
    data: lineData,
    title: {
      visible: true,
      text: "带数据点的折线图",
    },
    xField: "year",
    yField: "value",
    point: {
      visible: true,
      size: 5,
      shape: "diamond",
      style: {
        fill: "white",
        stroke: "#2593fc",
        lineWidth: 2,
      },
    },
  };
  const tradesColumns = [
    { title: "Trade ID", dataIndex: "tradeId", key: "tradeId" },
    { title: "Date", dataIndex: "date", key: "date" },
    { title: "Symbol", dataIndex: "symbol", key: "symbol" },
    { title: "Type", dataIndex: "type", key: "type" },
    { title: "Amount", dataIndex: "amount", key: "amount" },
    { title: "Price", dataIndex: "price", key: "price" },
  ];

  const balancesColumns = [
    { title: "Asset", dataIndex: "asset", key: "asset" },
    { title: "Balance", dataIndex: "balance", key: "balance" },
    { title: "Value", dataIndex: "value", key: "value" },
  ];

  const tradesData = [
    {
      key: "1",
      tradeId: "1",
      date: "2024-07-20",
      symbol: "BTC",
      type: "Buy",
      amount: "0.5",
      price: "50000",
    },
    {
      key: "2",
      tradeId: "2",
      date: "2024-07-21",
      symbol: "ETH",
      type: "Sell",
      amount: "1.0",
      price: "2500",
    },
  ];

  const balancesData = [
    { key: "1", asset: "BTC", balance: "1.5", value: "75000" },
    { key: "2", asset: "ETH", balance: "10", value: "25000" },
    { key: "3", asset: "ETH", balance: "10", value: "25000" },
    { key: "4", asset: "ETH", balance: "10", value: "25000" },
    { key: "5", asset: "ETH", balance: "10", value: "25000" },
    { key: "6", asset: "ETH", balance: "10", value: "25000" },
    { key: "7", asset: "ETH", balance: "10", value: "25000" },
    { key: "8", asset: "ETH", balance: "10", value: "25000" },
    { key: "9", asset: "ETH", balance: "10", value: "25000" },
    { key: "10", asset: "ETH", balance: "10", value: "25000" },
  ];

  const tabItems = [
    {
      key: "1",
      label: "Trades",
      children: (
        <Table
          columns={tradesColumns}
          dataSource={tradesData}
          pagination={false}
        />
      ),
    },
    {
      key: "2",
      label: "Balances",
      children: (
        <Table
          columns={balancesColumns}
          dataSource={balancesData}
          pagination={false}
        />
      ),
    },
  ];

  return (
    <div style={styles.body}>
      <h1 style={styles.header}>Profile</h1>
      <div style={styles.chartsContainer}>
        <div style={styles.chartWrapper}>
          <Pie {...config} />
        </div>
        <div style={styles.chartWrapper}>
          <Line {...lineConfig} />
        </div>
      </div>
      <div style={styles.tableContainer}>
        <Tabs defaultActiveKey="1" items={tabItems}></Tabs>
      </div>
    </div>
  );
};

const styles = {
  body: {
    width: "100%",
    height: "100%",
  },
  header: {},
  chartsContainer: {
    display: "flex",
    width: "100%",
    justifyContent: "space-between",
  },
  chartWrapper: {
    flex: "1 1 50%",
    padding: "10px",
    margin: "5px",
    borderRadius: "10px",
    boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)",
    backgroundColor: "white", // Add background color to ensure shadow visibility
    overflow: "hidden", // Ensure the rounded border is applied correctly
  },
  tableContainer: {
    marginTop: "20px",
    marginLeft: "5px",
    marginRight: "5px",
    padding: "10px",
    backgroundColor: "white",
    borderRadius: "10px",
    boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)",
  },
};

export default Profile;
