import React, { useState, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Calendar, AlertCircle } from 'lucide-react';

interface DateTimeInputProps {
  value?: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  label?: string;
}

export default function DateTimeInput({ 
  value = '', 
  onChange, 
  placeholder = "e.g., March 15, 1423 14:30", 
  disabled = false,
  label = "In-Game Date & Time"
}: DateTimeInputProps) {
  const [inputValue, setInputValue] = useState(value);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    setInputValue(value);
  }, [value]);

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

  const parseDateTime = (input: string): { isValid: boolean; error?: string } => {
    if (!input.trim()) {
      return { isValid: false, error: 'Date and time cannot be empty' };
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
      /^(\d+)-(\d{1,2})-(\d{1,2})$/
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

    for (const pattern of patterns) {
      const match = input.match(pattern);
      if (match) {
        matched = true;
        
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

    if (!matched) {
      return { 
        isValid: false, 
        error: 'Invalid date format. Use formats like "March 15, 1423 14:30", "03/15/1423 14:30", or "1423-03-15 14:30"' 
      };
    }

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

    return { isValid: true };
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setInputValue(newValue);
    
    if (newValue.trim()) {
      const validation = parseDateTime(newValue);
      if (validation.isValid) {
        setError('');
        onChange(newValue);
      } else {
        setError(validation.error || 'Invalid date format');
      }
    } else {
      setError('');
      onChange('');
    }
  };

  const handleBlur = () => {
    if (inputValue.trim()) {
      const validation = parseDateTime(inputValue);
      if (!validation.isValid) {
        setError(validation.error || 'Invalid date format');
      }
    }
  };

  return (
    <div className="space-y-2">
      <Label className="text-white flex items-center gap-2">
        <Calendar className="h-4 w-4" />
        {label}
      </Label>
      <Input
        type="text"
        value={inputValue}
        onChange={handleInputChange}
        onBlur={handleBlur}
        placeholder={placeholder}
        disabled={disabled}
        className={`bg-white/10 border-white/20 text-white placeholder:text-purple-300 h-12 ${
          error ? 'border-red-400 focus:border-red-400' : ''
        }`}
      />
      {error && (
        <Alert className="bg-red-500/10 border-red-400/20">
          <AlertCircle className="h-4 w-4 text-red-400" />
          <AlertDescription className="text-red-300">
            {error}
          </AlertDescription>
        </Alert>
      )}
      <div className="text-xs text-purple-300">
        Supported formats: "March 15, 1423 14:30", "03/15/1423 14:30", "1423-03-15 14:30"
      </div>
    </div>
  );
}
