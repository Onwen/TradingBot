import {
  DownOutlined,
  MinusCircleOutlined,
  UpOutlined,
} from "@ant-design/icons";
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

const StrategyConfig: React.FC<{
  field: FormListFieldData;
  strategies: Array<{ value: string; label: string }>;
  remove: () => void;
}> = (props) => {
  const [expand, setExpand] = useState(false);

  const toggleExpand = () => {
    setExpand(!expand);
  };

  return (
    <div key={props.field.key} style={{ marginBottom: 24 }}>
      <Space
        style={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
        }}
        align="baseline"
      >
        <Form.Item
          {...props.field}
          name={[props.field.name, "strategyName"]}
          label="Strategy Name"
          rules={[
            {
              required: true,
              message: "Please select a strategy name!",
            },
          ]}
          style={{ flex: 1 }}
        >
          <Select
            placeholder="Select strategy"
            options={props.strategies}
          ></Select>
        </Form.Item>

        <Form.Item
          {...props.field}
          name={[props.field.name, "weight"]}
          label="Weight"
          rules={[
            {
              required: true,
              message: "Please input the weight!",
            },
          ]}
          style={{ flex: 1 }}
        >
          <InputNumber min={0} max={1} step={0.1} placeholder="Enter weight" />
        </Form.Item>

        <div style={styles.buttonContainer}>
          <Button type="primary" danger onClick={() => props.remove()}>
            Remove
          </Button>
          <Button type="primary" onClick={() => toggleExpand()}>
            {expand ? "Hide" : "Expand"}
          </Button>
        </div>
      </Space>
      {expand && (
        <Form.Item
          {...props.field}
          name={[props.field.name, "config"]}
          label="Configuration"
          rules={[
            {
              required: true,
              message: "Please input the configuration!",
            },
          ]}
        >
          <Input.TextArea placeholder="Enter configuration as JSON" rows={2} />
        </Form.Item>
      )}
    </div>
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

export default StrategyConfig;
