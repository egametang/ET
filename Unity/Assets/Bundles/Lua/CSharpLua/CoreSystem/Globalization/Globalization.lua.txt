local System = System
local emptyFn = System.emptyFn
local define = System.define

define("System.Globalization.NumberFormatInfo", {
  getInvariantInfo = emptyFn,
  getCurrentInfo = emptyFn,
})

define("System.Globalization.CultureInfo", {
  getInvariantCulture = emptyFn,
})

define("System.Globalization.DateTimeFormatInfo", {
  getInvariantInfo = emptyFn,
})