export interface DateValidationResult {
  isValid: boolean;
  error?: string;
}

const isLeapYear = (year: number): boolean => {
  return (year % 4 === 0 && year % 100 !== 0) || (year % 400 === 0);
};

const getDaysInMonth = (month: number, year: number): number => {
  const daysInMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
  if (month === 2 && isLeapYear(year)) {
    return 29;
  }
  return daysInMonth[month - 1];
};

export const validateInGameDateTime = (input: string): DateValidationResult => {
  if (!input.trim()) {
    return { isValid: true }; // Empty is valid for optional fields
  }

  // Try to parse various date formats
  const patterns = [
    // Month Day, Year Time (e.g., "March 15, 1423 14:30")
    /^([A-Za-z]+)\s+(\d+),?\s+(\d+)\s+(\d{1,2}):(\d{2})$/,
    // MM/DD/YYYY HH:MM
    /^(\d{1,2})\/(\d{1,2})\/(\d+)\s+(\d{1,2}):(\d{2})$/,
    // DD/MM/YYYY HH:MM
    /^(\d{1,2})\/(\d{1,2})\/(\d+)\s+(\d{1,2}):(\d{2})$/,
    // YYYY-MM-DD HH:MM
    /^(\d+)-(\d{1,2})-(\d{1,2})\s+(\d{1,2}):(\d{2})$/,
    // Just date without time (Month Day, Year)
    /^([A-Za-z]+)\s+(\d+),?\s+(\d+)$/,
    // Just date without time (MM/DD/YYYY)
    /^(\d{1,2})\/(\d{1,2})\/(\d+)$/,
    // Just date without time (YYYY-MM-DD)
    /^(\d+)-(\d{1,2})-(\d{1,2})$/,
    // Free-form text (for fantasy dates like "15th day of Mirtul, 1372 DR, evening")
    /^.+$/
  ];

  const monthNames = [
    'january', 'february', 'march', 'april', 'may', 'june',
    'july', 'august', 'september', 'october', 'november', 'december'
  ];

  // Initialize variables with default values
  let day: number = 0;
  let month: number = 0;
  let year: number = 0;
  let hour: number = 0;
  let minute: number = 0;
  let matched = false;
  let isStructuredDate = false;

  // Try structured date patterns first
  for (let i = 0; i < patterns.length - 1; i++) {
    const pattern = patterns[i];
    const match = input.match(pattern);
    if (match) {
      matched = true;
      isStructuredDate = true;
      
      if (pattern.source.includes('[A-Za-z]+')) {
        // Month name format
        const monthName = match[1].toLowerCase();
        const monthIndex = monthNames.findIndex(name => name.startsWith(monthName.toLowerCase()));
        
        if (monthIndex === -1) {
          return { isValid: false, error: 'Invalid month name. Use full month names like "January", "February", etc.' };
        }
        
        month = monthIndex + 1;
        day = parseInt(match[2]);
        year = parseInt(match[3]);
        
        if (match[4] && match[5]) {
          hour = parseInt(match[4]);
          minute = parseInt(match[5]);
        }
      } else if (pattern.source.startsWith('^(\\d+)-(\\d{1,2})-(\\d{1,2})')) {
        // YYYY-MM-DD format
        year = parseInt(match[1]);
        month = parseInt(match[2]);
        day = parseInt(match[3]);
        
        if (match[4] && match[5]) {
          hour = parseInt(match[4]);
          minute = parseInt(match[5]);
        }
      } else {
        // MM/DD/YYYY or DD/MM/YYYY format - assume MM/DD/YYYY for now
        month = parseInt(match[1]);
        day = parseInt(match[2]);
        year = parseInt(match[3]);
        
        if (match[4] && match[5]) {
          hour = parseInt(match[4]);
          minute = parseInt(match[5]);
        }
      }
      break;
    }
  }

  // If no structured pattern matched, accept as free-form text
  if (!matched) {
    return { isValid: true }; // Accept any text for fantasy dates
  }

  // Only validate structured dates
  if (isStructuredDate) {
    // Validate numeric values
    if (isNaN(day) || isNaN(month) || isNaN(year) || isNaN(hour) || isNaN(minute)) {
      return { isValid: false, error: 'All date and time components must be valid numbers' };
    }

    // Check for negative values
    if (day < 0 || month < 0 || year < 0 || hour < 0 || minute < 0) {
      return { isValid: false, error: 'Date and time values cannot be negative' };
    }

    // Check for non-integer values (this is handled by parseInt, but let's be explicit)
    if (!Number.isInteger(day) || !Number.isInteger(month) || !Number.isInteger(year) || 
        !Number.isInteger(hour) || !Number.isInteger(minute)) {
      return { isValid: false, error: 'Date and time values must be whole numbers' };
    }

    // Validate ranges
    if (month < 1 || month > 12) {
      return { isValid: false, error: 'Month must be between 1 and 12' };
    }

    if (year < 1) {
      return { isValid: false, error: 'Year must be a positive number' };
    }

    const maxDays = getDaysInMonth(month, year);
    if (day < 1 || day > maxDays) {
      const monthName = monthNames[month - 1];
      const leapYearNote = month === 2 && isLeapYear(year) ? ' (leap year)' : '';
      return { 
        isValid: false, 
        error: `Day must be between 1 and ${maxDays} for ${monthName.charAt(0).toUpperCase() + monthName.slice(1)} ${year}${leapYearNote}` 
      };
    }

    if (hour > 23) {
      return { isValid: false, error: 'Hour must be between 0 and 23 (24-hour format)' };
    }

    if (minute > 59) {
      return { isValid: false, error: 'Minutes must be between 0 and 59' };
    }
  }

  return { isValid: true };
};
