import {
  DownOutlined,
  MinusCircleOutlined,
  PlusOutlined,
  UpOutlined,
} from "@ant-design/icons";
import { Content } from "antd/es/layout/layout";
import {
  Button,
  Form,
  FormListFieldData,
  Input,
  InputNumber,
  Select,
  Space,
} from "antd";
import React, { useState } from "react";
import { useRouter } from "next/navigation";
import StrategyConfig from "./strategy-config";

const StrategyList: React.FC<{}> = (props) => {
  const [form] = Form.useForm();
  const router = useRouter();

  const strategies = [
    { value: "Fixed Allocation", label: "Fixed Allocation" },
    { value: "Dynamic Allocation", label: "Dynamic Allocation" },
    { value: "Momentum Strategy", label: "Momentum Strategy" },
  ];

  const onFinish = (values: any) => {
    console.log("Form values: ", values);
    // Implement your form submission logic here
  };

  return (
    <Content style={{ padding: "50px", margin: "0 auto", maxWidth: "800px" }}>
      <h1>Strategy Configuration</h1>
      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        initialValues={{
          strategies: [{ strategyName: "", weight: 1, config: {} }],
        }}
      >
        <Form.List name="strategies">
          {(fields, { add, remove }) => (
            <>
              {fields.map((field, index) => (
                <StrategyConfig
                  key={field.key}
                  field={field}
                  remove={() => remove(field.name)}
                  strategies={strategies}
                />
              ))}
              <Form.Item>
                <Button
                  type="dashed"
                  onClick={() => add()}
                  icon={<PlusOutlined />}
                  style={{ width: "100%" }}
                >
                  Add Strategy
                </Button>
              </Form.Item>
            </>
          )}
        </Form.List>

        <Form.Item>
          <Button type="primary" htmlType="submit">
            Save Configuration
          </Button>
          <Button
            type="default"
            onClick={() => router.back()}
            style={{ marginLeft: "10px" }}
          >
            Cancel
          </Button>
        </Form.Item>
      </Form>
    </Content>
  );
};

const styles = {
  container: {
    display: "flex",
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#f0f0f0",
  },
  counter: {
    display: "flex",
    alignItems: "center",
  },
  buttonContainer: {
    display: "flex",
    flexDirection: "column",
  },
};

export default StrategyList;
