import type { Config } from "tailwindcss";
const config: Config = {
  content: ["./src/**/*.{ts,tsx}"],
  theme: { extend: { colors: { brand: { DEFAULT: "#0c831f", dark: "#0a6a19", soft: "#e8f5ea" } } } },
  plugins: []
};
export default config;
