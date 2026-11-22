// The shapes of the server answers.
// Big numbers (up to 20!) arrive as strings, because a JavaScript number
// cannot hold them exactly.

export interface StartResponse {
  sessionId: string;
  n: number;
  totalCount: string;
  totalCountText: string;
}

export interface CombinationItem {
  number: string;
  combination: number[];
}

export interface NextResponse {
  hasCombination: boolean;
  item: CombinationItem | null;
  message: string | null;
}

export interface PageResponse {
  pageNumber: string;
  pageSize: number;
  totalPages: string;
  items: CombinationItem[];
  message: string | null;
}

/** Which screen is shown. */
export type Screen = 'start' | 'single' | 'all';
