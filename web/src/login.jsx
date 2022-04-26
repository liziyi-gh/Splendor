import React from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import "./login.css";

class __LoginForm extends React.Component {
  constructor(props) {
    super(props);
    this.state = { username: "", password: "" };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.loginUrl = "/api/login.json";
  }

  handleChange(event) {
    const target = event.target;
    const input_type = target.name;
    this.setState({ [input_type]: event.target.value });
  }

  handleSubmit(event) {
    axios.get(this.loginUrl).then(
      (res) => {
        const loginFeedback = res.data["loginFeedback"];
        if (loginFeedback === true) {
          this.props.navigate("gamepage", { replace: true });
        }
      },
      (res) => {
        alert("login timeout, please ensure you connected to internet");
      }
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

function LoginForm(props) {
  let navigate = useNavigate();

  return <__LoginForm {...props} navigate={navigate} />;
}

export default LoginForm;
