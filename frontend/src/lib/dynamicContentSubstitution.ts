import { MetadataNote } from '../backend';

/**
 * Processes dynamic content substitution in note content by replacing placeholders
 * with values from linked metadata notes.
 * 
 * Supports:
 * - Simple field access: <---fieldName--->
 * - Nested field access: <---object.field--->
 * - Array access: <---array[0].field--->
 */
export function processContentSubstitution(
  content: string,
  metadataNotes: MetadataNote[]
): string {
  if (!content || metadataNotes.length === 0) {
    return content;
  }

  // Regular expression to match placeholders: <---anything--->
  const placeholderRegex = /<---([^-]+)--->/g;
  
  return content.replace(placeholderRegex, (match, fieldPath) => {
    const trimmedPath = fieldPath.trim();
    
    // Try to find the value in any of the metadata notes
    for (const metadataNote of metadataNotes) {
      try {
        const parsedContent = JSON.parse(metadataNote.content);
        const value = getNestedValue(parsedContent, trimmedPath);
        
        if (value !== undefined && value !== null) {
          // Convert the value to a string representation
          if (typeof value === 'object') {
            return JSON.stringify(value);
          }
          return String(value);
        }
      } catch (error) {
        // Skip this metadata note if JSON parsing fails
        continue;
      }
    }
    
    // If no value found, return the original placeholder
    return match;
  });
}

/**
 * Gets a nested value from an object using dot notation and array access
 * Examples:
 * - "name" -> obj.name
 * - "character.name" -> obj.character.name
 * - "items[0].name" -> obj.items[0].name
 * - "stats.strength" -> obj.stats.strength
 */
function getNestedValue(obj: any, path: string): any {
  if (!obj || typeof obj !== 'object') {
    return undefined;
  }

  // Split the path by dots, but handle array notation
  const parts = path.split(/\.|\[|\]/).filter(part => part !== '');
  
  let current = obj;
  
  for (const part of parts) {
    if (current === null || current === undefined) {
      return undefined;
    }
    
    // Check if this part is a number (array index)
    const index = parseInt(part, 10);
    if (!isNaN(index) && Array.isArray(current)) {
      current = current[index];
    } else if (typeof current === 'object' && part in current) {
      current = current[part];
    } else {
      return undefined;
    }
  }
  
  return current;
}

/**
 * Checks if content contains any dynamic content placeholders
 */
export function hasPlaceholders(content: string): boolean {
  const placeholderRegex = /<---([^-]+)--->/g;
  return placeholderRegex.test(content);
}

/**
 * Extracts all placeholder field paths from content
 */
export function extractPlaceholders(content: string): string[] {
  const placeholderRegex = /<---([^-]+)--->/g;
  const placeholders: string[] = [];
  let match;
  
  while ((match = placeholderRegex.exec(content)) !== null) {
    placeholders.push(match[1].trim());
  }
  
  return placeholders;
}
