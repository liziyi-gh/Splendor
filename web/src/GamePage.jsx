import React from "react";
import "./GamePage.css";
import Button from "@mui/material/Button";

export class GamePage extends React.Component {
  render() {
    return (
      <div className="GamePage-background" style={{ height: "100vh" }}>
        <Button variant="contained">Hello World</Button>
      </div>
    );
  }
}
