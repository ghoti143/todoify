import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { RegisterForm } from "./RegisterForm";
import { createAuthWrapper } from "../../test/wrapper";

const renderRegisterForm = (registerFn = vi.fn(), onSwitch = vi.fn()) => {
  const wrapper = createAuthWrapper({ register: registerFn });
  return render(<RegisterForm onSwitch={onSwitch} />, { wrapper });
};

describe("RegisterForm", () => {
  it("renders display name, email, password fields and submit button", () => {
    renderRegisterForm();
    expect(screen.getByPlaceholderText("Jane Doe")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("you@example.com")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("••••••••")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /create account/i }),
    ).toBeInTheDocument();
  });

  it("calls register with all fields", async () => {
    const register = vi.fn().mockResolvedValue(undefined);
    renderRegisterForm(register);

    await userEvent.type(screen.getByPlaceholderText("Jane Doe"), "Jane Doe");
    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "jane@example.com",
    );
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "password123",
    );
    await userEvent.click(
      screen.getByRole("button", { name: /create account/i }),
    );

    expect(register).toHaveBeenCalledWith(
      "jane@example.com",
      "password123",
      "Jane Doe",
    );
  });

  it("calls register with undefined displayName when left empty", async () => {
    const register = vi.fn().mockResolvedValue(undefined);
    renderRegisterForm(register);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "jane@example.com",
    );
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "password123",
    );
    await userEvent.click(
      screen.getByRole("button", { name: /create account/i }),
    );

    expect(register).toHaveBeenCalledWith(
      "jane@example.com",
      "password123",
      undefined,
    );
  });

  it("shows inline error on failure without resetting fields", async () => {
    const register = vi
      .fn()
      .mockRejectedValue(new Error("Email already in use"));
    renderRegisterForm(register);

    const emailInput = screen.getByPlaceholderText("you@example.com");
    await userEvent.type(emailInput, "taken@example.com");
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "password123",
    );
    await userEvent.click(
      screen.getByRole("button", { name: /create account/i }),
    );

    expect(await screen.findByText("Email already in use")).toBeInTheDocument();
    expect(emailInput).toHaveValue("taken@example.com");
  });

  it("shows generic error when error has no message", async () => {
    const register = vi.fn().mockRejectedValue({});
    renderRegisterForm(register);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "a@b.com",
    );
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "password123",
    );
    await userEvent.click(
      screen.getByRole("button", { name: /create account/i }),
    );

    expect(await screen.findByText(/registration failed/i)).toBeInTheDocument();
  });

  it("disables submit button while loading", async () => {
    const register = vi.fn(() => new Promise(() => {}));
    renderRegisterForm(register);

    await userEvent.type(
      screen.getByPlaceholderText("you@example.com"),
      "a@b.com",
    );
    await userEvent.type(
      screen.getByPlaceholderText("••••••••"),
      "password123",
    );
    await userEvent.click(screen.getByTestId("submit-button"));

    expect(screen.getByTestId("submit-button")).toBeDisabled();
  });

  it("calls onSwitch when sign in link is clicked", async () => {
    const onSwitch = vi.fn();
    renderRegisterForm(vi.fn(), onSwitch);
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));
    expect(onSwitch).toHaveBeenCalledOnce();
  });
});
