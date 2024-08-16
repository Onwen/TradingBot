import React, { useState } from "react";

const Counter = () => {
  const [count, setCount] = useState(0);

  const increment = () => {
    setCount(count + 1);
  };

  const decrement = () => {
    setCount(count - 1);
  };

  return (
    <div>
      <h2>Simple Counter</h2>
      <div style={styles.counter}>
        <button onClick={decrement} style={styles.button}>
          -
        </button>
        <span>{count}</span>
        <button onClick={increment} style={styles.button}>
          +
        </button>
      </div>
    </div>
  );
};

const styles = {
  container: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    height: "100vh",
    backgroundColor: "#f0f0f0",
  },
  counter: {
    display: "flex",
    alignItems: "center",
  },
  button: {
    padding: "10px 20px",
    fontSize: "20px",
    margin: "0 10px",
    cursor: "pointer",
  },
  count: {
    fontSize: "24px",
    minWidth: "50px",
    textAlign: "center",
  },
};

export default Counter;
