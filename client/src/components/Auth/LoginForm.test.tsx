import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { LoginForm } from "./LoginForm";
import { createAuthWrapper } from "../../test/wrapper";

const renderLoginForm = (loginFn = vi.fn(), onSwitch = vi.fn()) => {
  const wrapper = createAuthWrapper({ login: loginFn });
  return render(<LoginForm onSwitch={onSwitch} />, { wrapper });
};

describe("LoginForm", () => {
  it("renders email, password fields and submit button", () => {
    renderLoginForm();
    expect(screen.getByPlaceholderText("you@example.com")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("••••••••")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /sign in/i }),
    ).toBeInTheDocument();
  });

  it("calls login with email and password on submit", async () => {
    const login = vi.fn().mockResolvedValue(undefined);
    renderLoginForm(login);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "user@example.com",
    );
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "password123",
    );
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));

    expect(login).toHaveBeenCalledWith("user@example.com", "password123");
  });

  it("shows inline error on login failure without resetting fields", async () => {
    const login = vi.fn().mockRejectedValue(new Error("Invalid credentials"));
    renderLoginForm(login);

    const emailInput = screen.getByPlaceholderText("you@example.com");
    await userEvent.type(emailInput, "user@example.com");
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "wrongpassword",
    );
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));

    expect(await screen.findByText("Invalid credentials")).toBeInTheDocument();
    expect(emailInput).toHaveValue("user@example.com");
  });

  it("shows generic error message when error has no message", async () => {
    const login = vi.fn().mockRejectedValue({});
    renderLoginForm(login);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "a@b.com",
    );
    await userEvent.type(screen.getByPlaceholderText("••••••••"), "pass");
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));

    expect(await screen.findByText(/login failed/i)).toBeInTheDocument();
  });

  it("disables submit button while loading", async () => {
    const login = vi.fn(() => new Promise(() => {})); // never resolves
    renderLoginForm(login);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "a@b.com",
    );
    await userEvent.type(screen.getByPlaceholderText("••••••••"), "pass");
    await userEvent.click(screen.getByTestId("submit-button"));

    expect(screen.getByTestId("submit-button")).toBeDisabled();
  });

  it("clears error on subsequent submit attempt", async () => {
    const login = vi
      .fn()
      .mockRejectedValueOnce(new Error("Invalid credentials"))
      .mockResolvedValue(undefined);
    renderLoginForm(login);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "a@b.com",
    );
    await userEvent.type(screen.getByPlaceholderText("••••••••"), "pass");
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));
    expect(await screen.findByText("Invalid credentials")).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));
    expect(screen.queryByText("Invalid credentials")).not.toBeInTheDocument();
  });

  it("calls onSwitch when register link is clicked", async () => {
    const onSwitch = vi.fn();
    renderLoginForm(vi.fn(), onSwitch);
    await userEvent.click(screen.getByRole("button", { name: /register/i }));
    expect(onSwitch).toHaveBeenCalledOnce();
  });
});
