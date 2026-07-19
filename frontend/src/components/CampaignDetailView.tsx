import React, { useState, useEffect } from 'react';
import { Campaign, Session, Participant, Role } from '../backend';
import { useGetSessions, useGetParticipants, useGetUserProfile, useUpdateCampaign, useDeleteCampaign } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import ParticipantsDialog from './ParticipantsDialog';
import CreateSessionDialog from './CreateSessionDialog';
import EditCampaignDialog from './EditCampaignDialog';
import SessionCard from './SessionCard';
import SessionDetailView from './SessionDetailView';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { ArrowLeft, Calendar, Users, Plus, Scroll, Crown, Gamepad2, Clock, Loader2, Info, Edit, Trash2, AlertTriangle } from 'lucide-react';

interface CampaignDetailViewProps {
  campaign: Campaign;
  isOwner: boolean;
  onBack: () => void;
}

interface ParticipantWithProfile extends Participant {
  name?: string;
}

export default function CampaignDetailView({ campaign, isOwner, onBack }: CampaignDetailViewProps) {
  const [sessions, setSessions] = useState<Session[]>([]);
  const [participants, setParticipants] = useState<ParticipantWithProfile[]>([]);
  const [selectedSession, setSelectedSession] = useState<Session | null>(null);
  const [isParticipantsDialogOpen, setIsParticipantsDialogOpen] = useState(false);
  const [isCreateSessionDialogOpen, setIsCreateSessionDialogOpen] = useState(false);
  const [isEditCampaignDialogOpen, setIsEditCampaignDialogOpen] = useState(false);
  const [isDeleteCampaignDialogOpen, setIsDeleteCampaignDialogOpen] = useState(false);
  const [loadingSessions, setLoadingSessions] = useState(false);
  const [loadingParticipants, setLoadingParticipants] = useState(false);
  const getSessions = useGetSessions();
  const getParticipants = useGetParticipants();
  const getUserProfile = useGetUserProfile();
  const deleteCampaign = useDeleteCampaign();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();

  // Check if current user is a GM
  const currentUserParticipant = participants.find(p => 
    p.principal.toString() === currentUserPrincipal?.toString()
  );
  const isCurrentUserGM = currentUserParticipant && 
    (currentUserParticipant.role === Role.gm || currentUserParticipant.role === Role.both);

  useEffect(() => {
    loadSessions();
    loadParticipants();
  }, [campaign.id]);

  const loadSessions = async () => {
    setLoadingSessions(true);
    try {
      const sessionList = await getSessions.mutateAsync(campaign.id);
      setSessions(sessionList);
    } catch (error) {
      console.error('Failed to load sessions:', error);
    } finally {
      setLoadingSessions(false);
    }
  };

  const loadParticipants = async () => {
    setLoadingParticipants(true);
    try {
      const participantList = await getParticipants.mutateAsync(campaign.id);
      const participantsWithProfiles = await Promise.all(
        participantList.map(async (participant) => {
          try {
            const profile = await getUserProfile.mutateAsync(participant.principal);
            return {
              ...participant,
              name: profile?.name,
            };
          } catch {
            return participant;
          }
        })
      );
      setParticipants(participantsWithProfiles);
    } catch (error) {
      console.error('Failed to load participants:', error);
    } finally {
      setLoadingParticipants(false);
    }
  };

  const handleSessionCreated = () => {
    loadSessions();
  };

  const handleSessionUpdated = () => {
    loadSessions();
  };

  const handleSessionClick = (session: Session) => {
    setSelectedSession(session);
  };

  const handleBackToSessions = () => {
    setSelectedSession(null);
  };

  const handleDeleteCampaign = () => {
    deleteCampaign.mutate(campaign.id, {
      onSuccess: () => {
        setIsDeleteCampaignDialogOpen(false);
        onBack(); // Go back to dashboard after successful deletion
      }
    });
  };

  // New callback to refresh participants when roles are updated
  const handleParticipantsUpdated = () => {
    loadParticipants();
  };

  const getRoleIcon = (role: Role) => {
    switch (role) {
      case Role.gm:
        return <Crown className="h-4 w-4 text-amber-400" />;
      case Role.player:
        return <Gamepad2 className="h-4 w-4 text-blue-400" />;
      case Role.both:
        return (
          <div className="flex items-center space-x-1">
            <Crown className="h-3 w-3 text-amber-400" />
            <Gamepad2 className="h-3 w-3 text-blue-400" />
          </div>
        );
    }
  };

  const getRoleText = (role: Role) => {
    switch (role) {
      case Role.gm:
        return 'GM';
      case Role.player:
        return 'Player';
      case Role.both:
        return 'GM & Player';
    }
  };

  // If a session is selected, show the session detail view
  if (selectedSession) {
    return (
      <SessionDetailView 
        session={selectedSession} 
        campaignParticipants={participants}
        onBack={handleBackToSessions}
        onSessionUpdated={handleSessionUpdated}
      />
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center space-x-4">
        <Button
          onClick={onBack}
          variant="outline"
          size="sm"
          className="border-white/20 text-white hover:bg-white/10"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>
        <div className="flex items-center space-x-3 flex-1">
          <Scroll className="h-8 w-8 text-amber-400" />
          <div>
            <h1 className="text-3xl font-bold text-white">{campaign.name}</h1>
            <div className="flex items-center space-x-2 text-purple-200">
              <Calendar className="h-4 w-4" />
              <span>{campaign.inGameDateTime}</span>
            </div>
          </div>
        </div>
        {/* Campaign Management Actions - Only for owners */}
        {isOwner && (
          <div className="flex items-center space-x-2">
            <Button
              onClick={() => setIsEditCampaignDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-white/20 text-white hover:bg-white/10"
            >
              <Edit className="h-4 w-4 mr-2" />
              Edit Campaign
            </Button>
            <Button
              onClick={() => setIsDeleteCampaignDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-red-400/30 text-red-400 hover:bg-red-500/10 hover:border-red-400"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete Campaign
            </Button>
          </div>
        )}
      </div>

      {/* Campaign Details */}
      <Card className="bg-white/10 backdrop-blur-sm border-white/20">
        <CardHeader>
          <CardTitle className="text-white flex items-center gap-2">
            <Scroll className="h-5 w-5 text-amber-400" />
            Campaign Details
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="text-sm font-medium text-purple-300 mb-2">Description</h4>
            <p className="text-white whitespace-pre-wrap">
              {campaign.description || 'No description provided.'}
            </p>
          </div>
          <Separator className="bg-white/20" />
          <div className="flex items-center justify-between">
            <div>
              <h4 className="text-sm font-medium text-purple-300 mb-2">Participants</h4>
              <div className="flex items-center space-x-4">
                <span className="text-white">{participants.length} participant{participants.length === 1 ? '' : 's'}</span>
                {loadingParticipants ? (
                  <Loader2 className="h-4 w-4 animate-spin text-purple-400" />
                ) : (
                  <div className="flex items-center space-x-2">
                    {participants.slice(0, 3).map((participant, index) => (
                      <div key={participant.principal.toString()} className="flex items-center space-x-1">
                        {getRoleIcon(participant.role)}
                        <span className="text-xs text-purple-200">
                          {participant.name || 'Unknown'}
                        </span>
                      </div>
                    ))}
                    {participants.length > 3 && (
                      <span className="text-xs text-purple-300">+{participants.length - 3} more</span>
                    )}
                  </div>
                )}
              </div>
            </div>
            <Button
              onClick={() => setIsParticipantsDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-white/20 text-white hover:bg-white/10"
            >
              <Users className="h-4 w-4 mr-2" />
              {isOwner ? 'Manage Participants' : 'View Participants'}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Sessions Section */}
      <Card className="bg-white/10 backdrop-blur-sm border-white/20">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-white flex items-center gap-2">
              <Clock className="h-5 w-5 text-blue-400" />
              Sessions ({sessions.length})
            </CardTitle>
            {isCurrentUserGM && (
              <Button
                onClick={() => setIsCreateSessionDialogOpen(true)}
                size="sm"
                className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
              >
                <Plus className="h-4 w-4 mr-2" />
                Create Session
              </Button>
            )}
          </div>
          <CardDescription className="text-purple-200 flex items-start gap-2">
            <div className="flex-1">
              {isCurrentUserGM 
                ? "Create and manage sessions for your campaign. Click on any session to view details."
                : "View all sessions for this campaign. Click on any session to view details."
              }
            </div>
            {!isCurrentUserGM && (
              <div className="flex items-center gap-1 text-xs text-amber-300 bg-amber-500/10 px-2 py-1 rounded border border-amber-400/20">
                <Info className="h-3 w-3" />
                <span>Only GMs can create sessions</span>
              </div>
            )}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loadingSessions ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
              <span className="ml-2 text-purple-200">Loading sessions...</span>
            </div>
          ) : sessions.length === 0 ? (
            <div className="text-center py-12">
              <Clock className="h-16 w-16 text-purple-400 mx-auto mb-4" />
              <h4 className="text-lg font-semibold text-white mb-2">No Sessions Yet</h4>
              <p className="text-purple-200 mb-4 text-sm">
                {isCurrentUserGM 
                  ? "Create your first session to start tracking your campaign progress."
                  : "Sessions will appear here when GMs create them."
                }
              </p>
              {!isCurrentUserGM && (
                <div className="bg-amber-500/10 border border-amber-400/20 rounded-lg p-4 max-w-md mx-auto">
                  <div className="flex items-center justify-center gap-2 text-amber-300 mb-2">
                    <Crown className="h-4 w-4" />
                    <span className="text-sm font-medium">GM Access Required</span>
                  </div>
                  <p className="text-xs text-amber-200">
                    Only participants with GM role can create sessions. Ask a campaign owner to assign you the GM role if you need to create sessions.
                  </p>
                </div>
              )}
              {isCurrentUserGM && (
                <Button
                  onClick={() => setIsCreateSessionDialogOpen(true)}
                  size="sm"
                  className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Create First Session
                </Button>
              )}
            </div>
          ) : (
            <ScrollArea className="h-[400px] pr-4">
              <div className="space-y-4">
                {sessions.map((session) => (
                  <SessionCard 
                    key={session.id} 
                    session={session} 
                    participants={participants}
                    onSessionUpdated={handleSessionUpdated}
                    onSessionClick={handleSessionClick}
                  />
                ))}
              </div>
            </ScrollArea>
          )}
        </CardContent>
      </Card>

      {/* Dialogs */}
      <ParticipantsDialog
        campaign={campaign}
        open={isParticipantsDialogOpen}
        onOpenChange={setIsParticipantsDialogOpen}
        isOwner={isOwner}
        onParticipantsUpdated={handleParticipantsUpdated}
      />

      {isCurrentUserGM && (
        <CreateSessionDialog
          campaign={campaign}
          open={isCreateSessionDialogOpen}
          onOpenChange={setIsCreateSessionDialogOpen}
          onSessionCreated={handleSessionCreated}
        />
      )}

      {isOwner && (
        <>
          <EditCampaignDialog
            campaign={campaign}
            open={isEditCampaignDialogOpen}
            onOpenChange={setIsEditCampaignDialogOpen}
          />

          <AlertDialog open={isDeleteCampaignDialogOpen} onOpenChange={setIsDeleteCampaignDialogOpen}>
            <AlertDialogContent className="bg-gray-900 border-red-400/30 text-white max-w-md">
              <AlertDialogHeader className="space-y-4">
                <div className="flex items-center justify-center">
                  <div className="bg-red-500/20 p-3 rounded-full">
                    <AlertTriangle className="h-8 w-8 text-red-400" />
                  </div>
                </div>
                <AlertDialogTitle className="text-center text-xl font-bold text-white">
                  Delete Campaign
                </AlertDialogTitle>
                <AlertDialogDescription className="text-center text-purple-200 space-y-2">
                  <p className="font-medium">
                    Are you sure you want to delete <span className="text-white font-semibold">"{campaign.name}"</span>?
                  </p>
                  <p className="text-sm">
                    <span className="font-medium text-blue-300">In-Game Date:</span> {campaign.inGameDateTime}
                  </p>
                  <p className="text-sm text-red-300">
                    This action cannot be undone. The campaign and all its sessions will be permanently removed.
                  </p>
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
                <AlertDialogCancel 
                  className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
                  disabled={deleteCampaign.isPending}
                >
                  Cancel
                </AlertDialogCancel>
                <AlertDialogAction
                  onClick={handleDeleteCampaign}
                  disabled={deleteCampaign.isPending}
                  className="bg-red-600 hover:bg-red-700 text-white border-0 order-1 sm:order-2"
                >
                  {deleteCampaign.isPending ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Deleting...
                    </>
                  ) : (
                    <>
                      <Trash2 className="h-4 w-4 mr-2" />
                      Delete Campaign
                    </>
                  )}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </>
      )}
    </div>
  );
}
