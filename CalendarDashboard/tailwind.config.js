/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'media',
  content: [
    "./Pages/**/*.{cshtml,razor,html,js}",
    "./Views/**/*.{cshtml,razor,html,js}",
    "./wwwroot/**/*.js"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};