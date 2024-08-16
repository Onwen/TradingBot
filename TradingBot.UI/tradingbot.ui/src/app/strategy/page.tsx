"use client";
import React from "react";
import Sidebar from "../../components/sidebar/sidebar";
import StrategyList from "@/components/strategy/strategy-list";
const StrategyPage = () => {
  return (
    <Sidebar>
      <StrategyList />
    </Sidebar>
  );
};

export default StrategyPage;
