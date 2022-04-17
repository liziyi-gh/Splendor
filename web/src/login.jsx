import React from "react";
import "./login.css";

export class LoginForm extends React.Component {
  constructor(props) {
    super(props);
  }

  render() {
    return (
      <div>
        <label>
          用户名:
          <input type="text" name="name" />
          <br />
          密码:
          <input type="text" name="name" />
        </label>
        <br />
        <button className="Login-button-blue" type="submit">
          sign-in
        </button>
      </div>
    );
  }
}
