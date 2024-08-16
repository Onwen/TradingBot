"use client";
import React from "react";
import Sidebar from "../components/sidebar/sidebar";
import Profile from "../components/profile/profile";
export default function Home() {
  return (
    <>
      <Sidebar>
        <Profile></Profile>
      </Sidebar>
    </>
  );
}
