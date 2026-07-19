import React, { useState } from 'react';
import { useCreateMetadataNote } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, Database, AlertCircle, CheckCircle, Crown, Info } from 'lucide-react';

interface CreateMetadataNoteDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onMetadataNoteCreated?: () => void;
  campaignId: string;
  isSessionOwner?: boolean;
}

export default function CreateMetadataNoteDialog({
  open,
  onOpenChange,
  onMetadataNoteCreated,
  campaignId,
  isSessionOwner = false
}: CreateMetadataNoteDialogProps) {
  const [name, setName] = useState('');
  const [content, setContent] = useState('{\n  "character": {\n    "name": "Aragorn",\n    "level": 5,\n    "class": "Ranger"\n  },\n  "location": "Rivendell",\n  "items": [\n    {"name": "Sword", "damage": "1d8"},\n    {"name": "Bow", "damage": "1d6"}\n  ]\n}');
  const [linkedSessionNotes, setLinkedSessionNotes] = useState('');
  const [jsonError, setJsonError] = useState<string | null>(null);
  const [isValidJson, setIsValidJson] = useState(true);

  const createMetadataNote = useCreateMetadataNote();

  const validateJson = (jsonString: string) => {
    if (!jsonString.trim()) {
      setJsonError('JSON content cannot be empty');
      setIsValidJson(false);
      return;
    }

    try {
      JSON.parse(jsonString);
      setJsonError(null);
      setIsValidJson(true);
    } catch (error) {
      setJsonError(`Invalid JSON: ${error instanceof Error ? error.message : 'Unknown error'}`);
      setIsValidJson(false);
    }
  };

  const handleContentChange = (value: string) => {
    setContent(value);
    validateJson(value);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!name.trim()) {
      return;
    }

    if (!isValidJson) {
      return;
    }

    // Parse linked session notes (comma-separated note names)
    const linkedNotes = linkedSessionNotes
      .split(',')
      .map(note => note.trim())
      .filter(note => note.length > 0);

    const id = `metadata_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    createMetadataNote.mutate({
      id,
      name: name.trim(),
      content: content.trim(),
      linkedSessionNotes: linkedNotes,
      campaignId: campaignId // Pass campaignId instead of sessionId
    }, {
      onSuccess: () => {
        setName('');
        setContent('{\n  "character": {\n    "name": "Aragorn",\n    "level": 5,\n    "class": "Ranger"\n  },\n  "location": "Rivendell",\n  "items": [\n    {"name": "Sword", "damage": "1d8"},\n    {"name": "Bow", "damage": "1d6"}\n  ]\n}');
        setLinkedSessionNotes('');
        setJsonError(null);
        setIsValidJson(true);
        onOpenChange(false);
        onMetadataNoteCreated?.();
      }
    });
  };

  const handleCancel = () => {
    setName('');
    setContent('{\n  "character": {\n    "name": "Aragorn",\n    "level": 5,\n    "class": "Ranger"\n  },\n  "location": "Rivendell",\n  "items": [\n    {"name": "Sword", "damage": "1d8"},\n    {"name": "Bow", "damage": "1d6"}\n  ]\n}');
    setLinkedSessionNotes('');
    setJsonError(null);
    setIsValidJson(true);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Database className="h-6 w-6 text-purple-400" />
            Create Metadata Note
            {isSessionOwner && (
              <div className="flex items-center gap-1 text-xs text-green-300 bg-green-500/10 px-2 py-1 rounded border border-green-400/20 ml-2">
                <Crown className="h-3 w-3" />
                <span>Session Owner</span>
              </div>
            )}
          </DialogTitle>
          <DialogDescription className="text-purple-200">
            Create a metadata note with JSON content that can be linked to session notes for content substitution.
            Use placeholders like <code className="bg-black/30 px-1 py-0.5 rounded text-blue-100">&lt;---field---&gt;</code> in 
            session notes to reference this metadata.
            {isSessionOwner && (
              <span className="block mt-2 text-green-300 text-sm">
                <Crown className="h-3 w-3 inline mr-1" />
                As session owner, you can create metadata notes for this session regardless of note permissions.
              </span>
            )}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="name" className="text-white">
              Name <span className="text-red-400">*</span>
            </Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter metadata note name..."
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300"
              required
            />
            <p className="text-xs text-purple-300">
              Metadata note names must be unique within the campaign.
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="content" className="text-white">
              JSON Content <span className="text-red-400">*</span>
            </Label>
            <div className="space-y-2">
              <Textarea
                id="content"
                value={content}
                onChange={(e) => handleContentChange(e.target.value)}
                placeholder="Enter valid JSON content..."
                className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 min-h-[200px] font-mono text-sm"
                required
              />
              
              {/* JSON Validation Feedback */}
              {jsonError ? (
                <Alert className="border-red-400/30 bg-red-500/10">
                  <AlertCircle className="h-4 w-4 text-red-400" />
                  <AlertDescription className="text-red-300">
                    {jsonError}
                  </AlertDescription>
                </Alert>
              ) : isValidJson && content.trim() && (
                <Alert className="border-green-400/30 bg-green-500/10">
                  <CheckCircle className="h-4 w-4 text-green-400" />
                  <AlertDescription className="text-green-300">
                    Valid JSON format
                  </AlertDescription>
                </Alert>
              )}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="linkedNotes" className="text-white">
              Linked Session Notes (Optional)
            </Label>
            <Alert className="bg-blue-500/10 border-blue-400/20 mb-2">
              <Info className="h-4 w-4 text-blue-400" />
              <AlertDescription className="text-blue-200 text-sm">
                <strong>Note linking uses exact note names.</strong> Enter the exact names of session notes you want to link to this metadata note, separated by commas.
              </AlertDescription>
            </Alert>
            <Input
              id="linkedNotes"
              value={linkedSessionNotes}
              onChange={(e) => setLinkedSessionNotes(e.target.value)}
              placeholder="Enter Session Note names separated by commas..."
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300"
            />
            <p className="text-xs text-purple-300">
              Example: "Dragon's Lair Discovery, Ancient Artifact Found" - Make sure to use exact note names including capitalization and spacing.
            </p>
          </div>

          <Alert className="border-blue-400/30 bg-blue-500/10">
            <Database className="h-4 w-4 text-blue-400" />
            <AlertDescription className="text-blue-300">
              <strong>Content Substitution:</strong> Session notes can reference this metadata using 
              placeholders like <code className="bg-black/30 px-1 py-0.5 rounded">&lt;---character.name---&gt;</code> or 
              <code className="bg-black/30 px-1 py-0.5 rounded">&lt;---items[0].name---&gt;</code> for automatic content substitution.
            </AlertDescription>
          </Alert>

          <DialogFooter className="flex gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              className="border-white/20 text-white hover:bg-white/10"
              disabled={createMetadataNote.isPending}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={createMetadataNote.isPending || !name.trim() || !isValidJson}
              className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            >
              {createMetadataNote.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Creating...
                </>
              ) : (
                <>
                  <Database className="h-4 w-4 mr-2" />
                  Create Metadata Note
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
