import React, { useState } from 'react';
import { Campaign } from '../backend';
import { useLeaveCampaign } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import ParticipantsDialog from './ParticipantsDialog';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Badge } from '@/components/ui/badge';
import { Scroll, Calendar, Users, Loader2, Crown, Eye, LogOut, MoreVertical } from 'lucide-react';

interface CampaignCardProps {
  campaign: Campaign;
  isOwner?: boolean;
  onCampaignClick?: (campaign: Campaign) => void;
}

export default function CampaignCard({ campaign, isOwner, onCampaignClick }: CampaignCardProps) {
  const [isParticipantsDialogOpen, setIsParticipantsDialogOpen] = useState(false);
  const [isLeaveConfirmOpen, setIsLeaveConfirmOpen] = useState(false);
  const leaveCampaign = useLeaveCampaign();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isActualOwner = campaign.owner.toString() === currentUserPrincipal?.toString();

  // Use the isOwner prop if provided, otherwise determine from campaign owner
  const showOwnerActions = isOwner !== undefined ? isOwner : isActualOwner;

  const handleLeaveCampaign = () => {
    leaveCampaign.mutate(campaign.id);
    setIsLeaveConfirmOpen(false);
  };

  const handleCardClick = (e: React.MouseEvent) => {
    // Don't trigger card click if clicking on dropdown or buttons
    if ((e.target as HTMLElement).closest('[data-dropdown-trigger]') || 
        (e.target as HTMLElement).closest('button')) {
      return;
    }
    onCampaignClick?.(campaign);
  };

  return (
    <>
      <Card 
        className="bg-white/10 backdrop-blur-sm border-white/20 hover:bg-white/15 transition-all duration-200 group h-full cursor-pointer"
        onClick={handleCardClick}
      >
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-2 flex-1 min-w-0">
              <Scroll className="h-5 w-5 text-amber-400 flex-shrink-0" />
              <div className="min-w-0 flex-1">
                <CardTitle className="text-white text-lg truncate">{campaign.name}</CardTitle>
                <div className="flex items-center space-x-2 mt-1">
                  {!showOwnerActions && (
                    <Badge variant="outline" className="text-xs text-blue-300 border-blue-400/30">
                      <Users className="h-3 w-3 mr-1" />
                      Participant
                    </Badge>
                  )}
                  {showOwnerActions && (
                    <Badge variant="outline" className="text-xs text-amber-300 border-amber-400/30">
                      <Crown className="h-3 w-3 mr-1" />
                      Owner
                    </Badge>
                  )}
                </div>
              </div>
            </div>
            {/* Only show dropdown for non-owners (participants who can leave) */}
            {!showOwnerActions && (
              <DropdownMenu>
                <DropdownMenuTrigger asChild data-dropdown-trigger>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 w-8 p-0 text-purple-300 hover:text-white hover:bg-white/10 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0"
                  >
                    <MoreVertical className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="bg-gray-900 border-white/20">
                  <DropdownMenuItem
                    onClick={() => setIsParticipantsDialogOpen(true)}
                    className="text-white hover:bg-white/10 cursor-pointer"
                  >
                    <Eye className="h-4 w-4 mr-2" />
                    View Participants
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => setIsLeaveConfirmOpen(true)}
                    className="text-orange-400 hover:bg-orange-500/10 cursor-pointer"
                  >
                    <LogOut className="h-4 w-4 mr-2" />
                    Leave Campaign
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            )}
          </div>
          <div className="flex items-center space-x-2 text-sm text-purple-300 mt-2">
            <Calendar className="h-4 w-4 flex-shrink-0" />
            <span className="truncate">{campaign.inGameDateTime}</span>
          </div>
        </CardHeader>
        <CardContent>
          <CardDescription className="text-purple-200 line-clamp-3">
            {campaign.description || 'No description provided.'}
          </CardDescription>
        </CardContent>
      </Card>

      {/* Leave Campaign Confirmation Modal */}
      <AlertDialog open={isLeaveConfirmOpen} onOpenChange={setIsLeaveConfirmOpen}>
        <AlertDialogContent className="bg-gray-900 border-orange-400/30 text-white max-w-md">
          <AlertDialogHeader className="space-y-4">
            <div className="flex items-center justify-center">
              <div className="bg-orange-500/20 p-3 rounded-full">
                <LogOut className="h-8 w-8 text-orange-400" />
              </div>
            </div>
            <AlertDialogTitle className="text-center text-xl font-bold text-white">
              Leave Campaign
            </AlertDialogTitle>
            <AlertDialogDescription className="text-center text-purple-200 space-y-2">
              <p className="font-medium">
                Are you sure you want to leave <span className="text-white font-semibold">"{campaign.name}"</span>?
              </p>
              <p className="text-sm text-orange-300">
                You will lose access to this campaign immediately. You can only rejoin if the campaign owner invites you again.
              </p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
            <AlertDialogCancel 
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
              disabled={leaveCampaign.isPending}
            >
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleLeaveCampaign}
              disabled={leaveCampaign.isPending}
              className="bg-orange-600 hover:bg-orange-700 text-white border-0 order-1 sm:order-2"
            >
              {leaveCampaign.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Leaving...
                </>
              ) : (
                <>
                  <LogOut className="h-4 w-4 mr-2" />
                  Leave Campaign
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <ParticipantsDialog
        campaign={campaign}
        open={isParticipantsDialogOpen}
        onOpenChange={setIsParticipantsDialogOpen}
        isOwner={showOwnerActions}
      />
    </>
  );
}
