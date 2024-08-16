import React, { useState } from "react";
import { Layout, Menu } from "antd";
import { FundOutlined, HomeOutlined, SlidersOutlined } from "@ant-design/icons";
import type { MenuProps } from "antd";
import { useRouter, usePathname } from "next/navigation";

const { Header, Sider, Content } = Layout;

const Sidebar: React.FC<{ children?: React.ReactNode }> = (props) => {
  const router = useRouter();
  const pathname = usePathname(); // Get the current path
  const [collapsed, setCollapsed] = useState(false);

  const toggle = () => {
    setCollapsed(!collapsed);
  };

  type MenuItem = Required<MenuProps>["items"][number];
  const items: MenuItem[] = [
    {
      key: "/",
      label: "Home",
      icon: <HomeOutlined />,
      onClick: () => {
        router.push("/", { scroll: false });
      },
    },
    {
      key: "/strategy",
      label: "Strategy",
      icon: <FundOutlined />,
      onClick: () => {
        router.push("/strategy", { scroll: false });
      },
    },
    {
      key: "/backtest",
      label: "Backtest",
      icon: <SlidersOutlined />,
      onClick: () => {
        router.push("/backtest", { scroll: false });
      },
    },
  ];

  return (
    <Layout style={{ minHeight: "100vh" }}>
      <Sider trigger={null} collapsible collapsed={collapsed}>
        <div className="logo" />
        <Menu
          theme="dark"
          mode="inline"
          defaultSelectedKeys={["home"]}
          selectedKeys={[pathname]}
          items={items}
        ></Menu>
      </Sider>
      <div style={{ width: "100%" }}>{props.children}</div>
    </Layout>
  );
};

export default Sidebar;
