import React, { useState, KeyboardEvent } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Label } from '@/components/ui/label';
import { X, Plus, Tag } from 'lucide-react';

interface TagInputProps {
  tags: string[];
  onTagsChange: (tags: string[]) => void;
  label?: string;
  placeholder?: string;
  disabled?: boolean;
}

export default function TagInput({ 
  tags, 
  onTagsChange, 
  label = "Tags", 
  placeholder = "Add a tag...",
  disabled = false 
}: TagInputProps) {
  const [inputValue, setInputValue] = useState('');

  const addTag = (tag: string) => {
    const trimmedTag = tag.trim();
    if (trimmedTag && !tags.includes(trimmedTag)) {
      // Validate that tag is a single word (no spaces)
      if (trimmedTag.includes(' ')) {
        return false; // Invalid tag
      }
      onTagsChange([...tags, trimmedTag]);
      setInputValue('');
      return true;
    }
    return false;
  };

  const removeTag = (tagToRemove: string) => {
    onTagsChange(tags.filter(tag => tag !== tagToRemove));
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addTag(inputValue);
    } else if (e.key === 'Backspace' && inputValue === '' && tags.length > 0) {
      // Remove last tag when backspace is pressed on empty input
      removeTag(tags[tags.length - 1]);
    }
  };

  const handleAddClick = () => {
    addTag(inputValue);
  };

  return (
    <div className="space-y-2">
      <Label className="text-white flex items-center gap-2">
        <Tag className="h-4 w-4" />
        {label}
      </Label>
      
      {/* Display existing tags */}
      {tags.length > 0 && (
        <div className="flex flex-wrap gap-2 p-2 bg-white/5 rounded-lg border border-white/10">
          {tags.map((tag) => (
            <Badge
              key={tag}
              variant="secondary"
              className="bg-purple-500/20 text-purple-200 border-purple-400/30 hover:bg-purple-500/30 transition-colors"
            >
              {tag}
              {!disabled && (
                <button
                  type="button"
                  onClick={() => removeTag(tag)}
                  className="ml-1 hover:text-purple-100 transition-colors"
                >
                  <X className="h-3 w-3" />
                </button>
              )}
            </Badge>
          ))}
        </div>
      )}

      {/* Input for new tags */}
      {!disabled && (
        <div className="flex gap-2">
          <Input
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            className="bg-white/10 border-white/20 text-white placeholder:text-white/50"
          />
          <Button
            type="button"
            onClick={handleAddClick}
            disabled={!inputValue.trim() || inputValue.trim().includes(' ')}
            variant="outline"
            size="sm"
            className="border-white/20 text-white hover:bg-white/10 px-3"
          >
            <Plus className="h-4 w-4" />
          </Button>
        </div>
      )}

      <p className="text-xs text-purple-300">
        {disabled 
          ? "Tags help categorize and organize notes within campaigns."
          : "Press Enter or click + to add a tag. Tags must be single words without spaces."
        }
      </p>
    </div>
  );
}
