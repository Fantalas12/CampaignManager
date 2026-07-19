import React, { useState, useEffect } from 'react';
import { Campaign } from '../backend';
import { useUpdateCampaign } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Edit, Sparkles } from 'lucide-react';
import DateTimeInput from './DateTimeInput';

interface EditCampaignDialogProps {
  campaign: Campaign;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export default function EditCampaignDialog({ campaign, open, onOpenChange }: EditCampaignDialogProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [inGameDateTime, setInGameDateTime] = useState('');
  const updateCampaign = useUpdateCampaign();

  useEffect(() => {
    if (open) {
      setName(campaign.name);
      setDescription(campaign.description);
      setInGameDateTime(campaign.inGameDateTime);
    }
  }, [open, campaign]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (name.trim() && inGameDateTime.trim()) {
      updateCampaign.mutate(
        { id: campaign.id, name: name.trim(), description: description.trim(), inGameDateTime: inGameDateTime.trim() },
        {
          onSuccess: () => {
            onOpenChange(false);
          },
        }
      );
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setName(campaign.name);
      setDescription(campaign.description);
      setInGameDateTime(campaign.inGameDateTime);
    }
    onOpenChange(newOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Edit className="h-5 w-5 text-blue-400" />
            Edit Campaign
            <Sparkles className="h-4 w-4 text-purple-400" />
          </DialogTitle>
          <DialogDescription className="text-purple-200">
            Update your campaign details. Make changes to keep your adventure fresh.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="edit-campaign-name" className="text-white">Campaign Name</Label>
            <Input
              id="edit-campaign-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter campaign name"
              required
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 h-12"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-campaign-description" className="text-white">Description</Label>
            <Textarea
              id="edit-campaign-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe your campaign world, story, or setting..."
              rows={4}
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 resize-none"
            />
          </div>
          <DateTimeInput
            value={inGameDateTime}
            onChange={setInGameDateTime}
            placeholder="e.g., March 15, 1423 14:30"
          />
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => handleOpenChange(false)}
              className="border-white/20 text-white hover:bg-white/10"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={!name.trim() || !inGameDateTime.trim() || updateCampaign.isPending}
              className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            >
              {updateCampaign.isPending ? 'Updating...' : 'Update Campaign'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
