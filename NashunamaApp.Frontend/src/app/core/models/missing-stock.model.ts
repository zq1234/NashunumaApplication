export interface MissingStockDateDto {
  date: string;
  isMissing: boolean;
  displayDate?: string;
  Date?: string;
  IsMissing?: boolean;
  DisplayDate?: string;
}

export interface MissingStockNotificationDto {
  siteId: string;
  missingDates: MissingStockDateDto[];
  totalMissingDays: number;
  hasMissingEntries: boolean;
  message: string;
  SiteId?: string;
  MissingDates?: MissingStockDateDto[];
  TotalMissingDays?: number;
  HasMissingEntries?: boolean;
  Message?: string;
}

export function normalizeMissingStockNotification(input: any): MissingStockNotificationDto | null {
  if (!input) return null;

  const raw = Array.isArray(input) ? { missingDates: input } : input;
  const missingDatesRaw = Array.isArray(raw.missingDates) ? raw.missingDates : Array.isArray(raw.MissingDates) ? raw.MissingDates : [];

  const normalizedDates = missingDatesRaw.map((d: any) => ({
    date: d?.date ?? d?.Date ?? '',
    isMissing: d?.isMissing ?? d?.IsMissing ?? true,
    displayDate: d?.displayDate ?? d?.DisplayDate ?? (d?.date ?? d?.Date ?? ''),
    Date: d?.Date ?? d?.date ?? '',
    IsMissing: d?.IsMissing ?? d?.isMissing ?? true,
    DisplayDate: d?.DisplayDate ?? d?.displayDate ?? (d?.Date ?? d?.date ?? '')
  }));

  const totalMissingDays = Number(raw.totalMissingDays ?? raw.TotalMissingDays ?? normalizedDates.length ?? 0);
  const hasMissingEntries = raw.hasMissingEntries ?? raw.HasMissingEntries ?? normalizedDates.length > 0;

  return {
    siteId: raw.siteId ?? raw.SiteId ?? '',
    missingDates: normalizedDates,
    totalMissingDays: Number.isFinite(totalMissingDays) ? totalMissingDays : 0,
    hasMissingEntries: Boolean(hasMissingEntries),
    message: raw.message ?? raw.Message ?? (normalizedDates.length ? `You have ${normalizedDates.length} pending stock entries.` : 'No pending stock notifications.'),
    SiteId: raw.SiteId ?? raw.siteId ?? '',
    MissingDates: normalizedDates,
    TotalMissingDays: Number.isFinite(totalMissingDays) ? totalMissingDays : 0,
    HasMissingEntries: Boolean(hasMissingEntries),
    Message: raw.Message ?? raw.message ?? (normalizedDates.length ? `You have ${normalizedDates.length} pending stock entries.` : 'No pending stock notifications.')
  };
}
