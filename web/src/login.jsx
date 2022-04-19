import React from "react";
import { Router, Route, Link } from "react-router";
import { GamePage } from "./GamePage.jsx";
import "./login.css";

export class LoginForm extends React.Component {
  constructor(props) {
    super(props);
    this.state = { username: "", password: "" };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  handleChange(event) {
    const target = event.target;
    const input_type = target.name;
    this.setState({ [input_type]: event.target.value });
  }

  handleSubmit(event) {
    alert(
      "提交的用户名:" +
        this.state.username +
        "\n" +
        "提交的密码:" +
        this.state.password
    );
    event.preventDefault();
  }

  render() {
    return (
      <form onSubmit={this.handleSubmit}>
        <label>
          用户名:
          <input
            name="username"
            type="text"
            value={this.state.username}
            onChange={this.handleChange}
          />
          <br />
          密码:
          <input
            name="password"
            type="password"
            value={this.state.password}
            onChange={this.handleChange}
          />
          <br />
          <input className="Login-button-blue" type="submit" value="sign-in" />
        </label>
      </form>
    );
  }
}
