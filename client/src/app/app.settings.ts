/** Fixed settings of the client. */
export const AppSettings = {
  /** Address of the .NET server. */
  apiUrl: 'http://localhost:5271/api/combinations',

  /** How many combinations are shown on one page of the "all combinations" list. */
  pageSize: 10,

  /** How many page-number buttons the pager shows around the current page. */
  visiblePageNumbers: 5,

  minN: 1,
  maxN: 20,
} as const;
